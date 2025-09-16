using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VisitaBookingApi.Data;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Services.Implementation
{
    public class SimpleRoomService : IRoomService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IS3FileService _s3FileService;
    private readonly visita_booking_api.Services.Interfaces.IAvailabilityLedgerService _ledgerService;
    private readonly visita_booking_api.Services.Interfaces.ICacheInvalidationService _cacheInvalidation;
        private readonly ILogger<SimpleRoomService> _logger;

        public SimpleRoomService(
            ApplicationDbContext context,
            IMapper mapper,
            IS3FileService s3FileService,
            visita_booking_api.Services.Interfaces.IAvailabilityLedgerService ledgerService,
            visita_booking_api.Services.Interfaces.ICacheInvalidationService cacheInvalidation,
            ILogger<SimpleRoomService> logger)
        {
            _context = context;
            _mapper = mapper;
            _s3FileService = s3FileService;
            _ledgerService = ledgerService;
            _cacheInvalidation = cacheInvalidation;
            _logger = logger;
        }

        public async Task<RoomDetailsDTO?> GetByIdAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.Photos.Where(p => p.IsActive))
                .Include(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .Include(r => r.PricingRules.Where(pr => pr.IsActive))
                .Include(r => r.Accommodation)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            return room == null ? null : _mapper.Map<RoomDetailsDTO>(room);
        }

        public async Task<PaginatedResponse<RoomListItemDTO>> GetAllAsync(int page = 1, int pageSize = 10, bool includeInactive = false)
        {
            var query = _context.Rooms
                .Include(r => r.Photos.Where(p => p.IsActive))
                .Include(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .Include(r => r.Accommodation)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(r => r.IsActive);
            }

            var totalCount = await query.CountAsync();
            
            var rooms = await query
                .OrderByDescending(r => r.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var roomDtos = _mapper.Map<List<RoomListItemDTO>>(rooms);

            return PaginatedResponse<RoomListItemDTO>.Create(roomDtos, totalCount, page, pageSize);
        }

        public async Task<RoomDetailsDTO> CreateAsync(RoomCreateDTO createDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Validate amenity IDs
                if (createDto.AmenityIds.Any())
                {
                    var existingAmenityIds = await _context.Amenities
                        .Where(a => createDto.AmenityIds.Contains(a.Id) && a.IsActive)
                        .Select(a => a.Id)
                        .ToListAsync();

                    var invalidAmenityIds = createDto.AmenityIds.Except(existingAmenityIds);
                    if (invalidAmenityIds.Any())
                    {
                        throw new ArgumentException($"Invalid amenity IDs: {string.Join(", ", invalidAmenityIds)}");
                    }
                }

                // Create room entity
                var room = _mapper.Map<Room>(createDto);
                room.CreatedAt = DateTime.UtcNow;
                room.UpdateTimestamp();

                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();

                // Add amenities
                if (createDto.AmenityIds.Any())
                {
                    var roomAmenities = createDto.AmenityIds.Select(amenityId => new RoomAmenity
                    {
                        RoomId = room.Id,
                        AmenityId = amenityId,
                        AssignedAt = DateTime.UtcNow
                    }).ToList();

                    _context.RoomAmenities.AddRange(roomAmenities);
                    await _context.SaveChangesAsync();
                }

                // Handle photo uploads
                if (createDto.PhotoFiles != null && createDto.PhotoFiles.Any())
                {
                    var displayOrder = 0;
                    foreach (var photoFile in createDto.PhotoFiles)
                    {
                        try
                        {
                            var uploadResult = await _s3FileService.UploadFileAsync(photoFile, $"rooms/{room.Id}/photos");
                            if (uploadResult.Success && !string.IsNullOrEmpty(uploadResult.FileUrl))
                            {
                                var roomPhoto = new RoomPhoto
                                {
                                    RoomId = room.Id,
                                    S3Key = uploadResult.S3Key ?? "",
                                    S3Url = uploadResult.FileUrl,
                                    FileName = uploadResult.FileName ?? photoFile.FileName,
                                    FileSize = uploadResult.FileSize,
                                    ContentType = uploadResult.ContentType ?? photoFile.ContentType,
                                    DisplayOrder = displayOrder++,
                                    IsActive = true,
                                    UploadedAt = DateTime.UtcNow,
                                    LastModified = DateTime.UtcNow
                                };

                                _context.RoomPhotos.Add(roomPhoto);
                            }
                            else
                            {
                                _logger.LogWarning("Photo upload failed for room {RoomId}: {Error}", room.Id, uploadResult.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error uploading photo for room {RoomId}", room.Id);
                            // Continue with other photos even if one fails
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Warmup 6-month availability ledger for the newly created room (start = today, end exclusive = +6 months)
                try
                {
                    var start = DateTime.UtcNow.Date;
                    var endExclusive = start.AddMonths(6);
                    // Fire-and-forget warmup; we await to make sure consistency is achieved before returning created room
                    await _ledgerService.WarmupRoomLedgerAsync(room.Id, start, endExclusive);
                    await _cacheInvalidation.InvalidateAvailabilityCacheAsync(room.Id);
                }
                catch (Exception ex)
                {
                    // Log and continue; room creation should not fail due to ledger warmup issues
                    _logger.LogError(ex, "Failed to warmup availability ledger for new room {RoomId}", room.Id);
                }

                // Return created room details
                return await GetByIdAsync(room.Id) ?? throw new InvalidOperationException("Failed to retrieve created room");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<RoomDetailsDTO?> UpdateAsync(int roomId, RoomUpdateDTO updateDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var room = await _context.Rooms
                    .Include(r => r.RoomAmenities)
                    .Include(r => r.Photos)
                    .FirstOrDefaultAsync(r => r.Id == roomId);

                if (room == null) return null;

                // Update room properties
                _mapper.Map(updateDto, room);
                room.UpdateTimestamp();

                // Update amenities
                var existingAmenityIds = room.RoomAmenities.Select(ra => ra.AmenityId).ToList();
                var amenityIdsToAdd = updateDto.AmenityIds.Except(existingAmenityIds).ToList();
                var amenityIdsToRemove = existingAmenityIds.Except(updateDto.AmenityIds).ToList();

                // Remove old amenities
                if (amenityIdsToRemove.Any())
                {
                    var amenitiesToRemove = room.RoomAmenities
                        .Where(ra => amenityIdsToRemove.Contains(ra.AmenityId));
                    _context.RoomAmenities.RemoveRange(amenitiesToRemove);
                }

                // Add new amenities
                if (amenityIdsToAdd.Any())
                {
                    var newRoomAmenities = amenityIdsToAdd.Select(amenityId => new RoomAmenity
                    {
                        RoomId = room.Id,
                        AmenityId = amenityId,
                        AssignedAt = DateTime.UtcNow
                    });
                    _context.RoomAmenities.AddRange(newRoomAmenities);
                }

                // Handle photo management
                var currentPhotos = room.Photos.ToList();
                var photosToDelete = updateDto.PhotosToDelete ?? new List<int>();
                var photosToDeleteEntities = currentPhotos.Where(p => photosToDelete.Contains(p.Id)).ToList();

                // Delete specified photos from S3 and database
                foreach (var photo in photosToDeleteEntities)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(photo.S3Key))
                        {
                            await _s3FileService.DeleteFileAsync(photo.S3Key);
                        }
                        _context.RoomPhotos.Remove(photo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting photo {PhotoId} for room {RoomId}", photo.Id, roomId);
                        // Continue with other photos even if one fails to delete
                    }
                }

                // Upload new photos
                if (updateDto.PhotoFiles != null && updateDto.PhotoFiles.Any())
                {
                    var remainingPhotos = currentPhotos.Where(p => !photosToDelete.Contains(p.Id)).ToList();
                    var maxDisplayOrder = remainingPhotos.Any() ? remainingPhotos.Max(p => p.DisplayOrder) : -1;
                    var displayOrder = maxDisplayOrder + 1;

                    foreach (var photoFile in updateDto.PhotoFiles)
                    {
                        try
                        {
                            var uploadResult = await _s3FileService.UploadFileAsync(photoFile, $"rooms/{roomId}/photos");
                            if (uploadResult.Success && !string.IsNullOrEmpty(uploadResult.FileUrl))
                            {
                                var roomPhoto = new RoomPhoto
                                {
                                    RoomId = roomId,
                                    S3Key = uploadResult.S3Key ?? "",
                                    S3Url = uploadResult.FileUrl,
                                    FileName = uploadResult.FileName ?? photoFile.FileName,
                                    FileSize = uploadResult.FileSize,
                                    ContentType = uploadResult.ContentType ?? photoFile.ContentType,
                                    DisplayOrder = displayOrder++,
                                    IsActive = true,
                                    UploadedAt = DateTime.UtcNow,
                                    LastModified = DateTime.UtcNow
                                };

                                _context.RoomPhotos.Add(roomPhoto);
                            }
                            else
                            {
                                _logger.LogWarning("Photo upload failed for room {RoomId}: {Error}", roomId, uploadResult.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error uploading new photo for room {RoomId}", roomId);
                            // Continue with other photos even if one fails
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return updated room details
                return await GetByIdAsync(roomId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return false;

            // Soft delete
            room.IsActive = false;
            room.UpdateTimestamp();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveStatusAsync(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return false;

            room.IsActive = !room.IsActive;
            room.UpdateTimestamp();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RoomPhotoDTO>> GetPhotosAsync(int roomId)
        {
            var photos = await _context.RoomPhotos
                .Where(p => p.RoomId == roomId && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<RoomPhotoDTO>>(photos);
        }

        [Obsolete("Use CreateAsync or UpdateAsync with PhotoFiles property for integrated photo upload")]
        public async Task<RoomPhotoDTO> UploadPhotoAsync(int roomId, RoomPhotoUploadDTO uploadDto)
        {
            // Verify room exists
            if (!await ExistsAsync(roomId))
            {
                throw new ArgumentException($"Room with ID {roomId} not found");
            }

            // Upload to S3
            var uploadResponse = await _s3FileService.UploadFileAsync(uploadDto.File, $"rooms/{roomId}/photos");
            if (!uploadResponse.Success)
            {
                throw new InvalidOperationException($"Failed to upload file: {uploadResponse.Error}");
            }

            // Create photo record
            var photo = new RoomPhoto
            {
                RoomId = roomId,
                S3Key = uploadResponse.S3Key!,
                S3Url = uploadResponse.FileUrl!,
                FileName = uploadResponse.FileName!,
                FileSize = uploadResponse.FileSize,
                ContentType = uploadResponse.ContentType!,
                DisplayOrder = uploadDto.DisplayOrder,
                AltText = uploadDto.AltText,
                UploadedAt = DateTime.UtcNow,
            };
            photo.UpdateLastModified();

            _context.RoomPhotos.Add(photo);
            await _context.SaveChangesAsync();

            // Update room timestamp
            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                room.UpdateTimestamp();
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<RoomPhotoDTO>(photo);
        }

        public async Task<bool> DeletePhotoAsync(int roomId, int photoId)
        {
            var photo = await _context.RoomPhotos
                .FirstOrDefaultAsync(p => p.Id == photoId && p.RoomId == roomId);

            if (photo == null) return false;

            // Delete from S3
            await _s3FileService.DeleteFileAsync(photo.S3Key);

            // Delete from database
            _context.RoomPhotos.Remove(photo);
            await _context.SaveChangesAsync();

            // Update room timestamp
            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                room.UpdateTimestamp();
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> ReorderPhotosAsync(int roomId, List<int> photoIds)
        {
            var photos = await _context.RoomPhotos
                .Where(p => p.RoomId == roomId && photoIds.Contains(p.Id))
                .ToListAsync();

            if (photos.Count != photoIds.Count)
            {
                return false; // Some photo IDs don't belong to this room
            }

            // Update display order
            for (int i = 0; i < photoIds.Count; i++)
            {
                var photo = photos.First(p => p.Id == photoIds[i]);
                photo.DisplayOrder = i;
                photo.UpdateLastModified();
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AmenityDTO>> GetRoomAmenitiesAsync(int roomId)
        {
            var amenities = await _context.RoomAmenities
                .Where(ra => ra.RoomId == roomId)
                .Include(ra => ra.Amenity)
                .Where(ra => ra.Amenity.IsActive)
                .Select(ra => ra.Amenity)
                .OrderBy(a => a.Category)
                .ThenBy(a => a.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<AmenityDTO>>(amenities);
        }

        public async Task<bool> UpdateRoomAmenitiesAsync(int roomId, List<int> amenityIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate room exists
                if (!await ExistsAsync(roomId))
                {
                    return false;
                }

                // Remove all existing amenities for this room
                var existingRoomAmenities = await _context.RoomAmenities
                    .Where(ra => ra.RoomId == roomId)
                    .ToListAsync();

                _context.RoomAmenities.RemoveRange(existingRoomAmenities);

                // Add new amenities
                if (amenityIds.Any())
                {
                    var newRoomAmenities = amenityIds.Select(amenityId => new RoomAmenity
                    {
                        RoomId = roomId,
                        AmenityId = amenityId,
                        AssignedAt = DateTime.UtcNow
                    });

                    _context.RoomAmenities.AddRange(newRoomAmenities);
                }

                await _context.SaveChangesAsync();

                // Update room timestamp
                var room = await _context.Rooms.FindAsync(roomId);
                if (room != null)
                {
                    room.UpdateTimestamp();
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int roomId)
        {
            return await _context.Rooms.AnyAsync(r => r.Id == roomId);
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeRoomId = null)
        {
            var query = _context.Rooms.Where(r => r.Name == name);
            
            if (excludeRoomId.HasValue)
            {
                query = query.Where(r => r.Id != excludeRoomId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}