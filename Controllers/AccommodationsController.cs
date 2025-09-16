using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccommodationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IS3FileService _s3FileService;

        public AccommodationsController(ApplicationDbContext context, IS3FileService s3FileService)
        {
            _context = context;
            _s3FileService = s3FileService;
        }

        /// <summary>
        /// Get all active accommodations
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AccommodationSummaryDto>>> GetAccommodations()
        {
            var accommodations = await _context.Accommodations
                .Include(a => a.Owner)
                .Include(a => a.Rooms)
                .Where(a => a.IsActive)
                .Select(a => new AccommodationSummaryDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Logo = a.Logo,
                    Address = a.Address,
                    EmailAddress = a.EmailAddress,
                    ContactNo = a.ContactNo,
                    IsActive = a.IsActive,
                    Status = a.Status.ToString(),
                    ActiveRoomCount = a.Rooms.Count(r => r.IsActive)
                })
                .OrderBy(a => a.Name)
                .ToListAsync();

            return Ok(accommodations);
        }

        /// <summary>
        /// Get accommodation by id
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AccommodationResponseDto>> GetAccommodation(int id)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.Owner)
                .Include(a => a.Rooms)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

            if (accommodation == null)
            {
                return NotFound($"Accommodation with id {id} not found");
            }

            var response = new AccommodationResponseDto
            {
                Id = accommodation.Id,
                Name = accommodation.Name,
                Description = accommodation.Description,
                Logo = accommodation.Logo,
                Address = accommodation.Address,
                EmailAddress = accommodation.EmailAddress,
                ContactNo = accommodation.ContactNo,
                IsActive = accommodation.IsActive,
                Status = accommodation.Status.ToString(),
                ApprovedAt = accommodation.ApprovedAt,
                ApprovedById = accommodation.ApprovedById,
                RejectionReason = accommodation.RejectionReason,
                CreatedAt = accommodation.CreatedAt,
                UpdatedAt = accommodation.UpdatedAt,
                OwnerId = accommodation.OwnerId,
                OwnerName = accommodation.Owner?.FullName ?? "Unknown",
                OwnerEmail = accommodation.Owner?.Email ?? "Unknown",
                ActiveRoomCount = accommodation.Rooms.Count(r => r.IsActive)
            };

            // Generate presigned URLs for private business documents (short lived)
            var presignExpiration = TimeSpan.FromMinutes(15);
            if (!string.IsNullOrEmpty(accommodation.BusinessPermitS3Key))
            {
                response.BusinessPermitUrl = await _s3FileService.GetPresignedUrlAsync(accommodation.BusinessPermitS3Key, presignExpiration);
            }

            if (!string.IsNullOrEmpty(accommodation.DotAccreditationS3Key))
            {
                response.DotAccreditationUrl = await _s3FileService.GetPresignedUrlAsync(accommodation.DotAccreditationS3Key, presignExpiration);
            }

            // BTC membership is a boolean flag and not a document

            return Ok(response);
        }

        /// <summary>
        /// Get all accommodations owned by current user
        /// </summary>
        [HttpGet("my-accommodations")]
        public async Task<ActionResult<IEnumerable<AccommodationResponseDto>>> GetMyAccommodations()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var accommodations = await _context.Accommodations
                .Include(a => a.Owner)
                .Include(a => a.Rooms)
                .Where(a => a.OwnerId == currentUserId.Value)
                .ToListAsync();

            var presignExpiration = TimeSpan.FromMinutes(15);
            var results = new List<AccommodationResponseDto>();

            foreach (var a in accommodations.OrderBy(a => a.Name))
            {
                var dto = new AccommodationResponseDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Logo = a.Logo,
                    Address = a.Address,
                    EmailAddress = a.EmailAddress,
                    ContactNo = a.ContactNo,
                    IsActive = a.IsActive,
                    Status = a.Status.ToString(),
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    OwnerId = a.OwnerId,
                    OwnerName = a.Owner!.FullName,
                    OwnerEmail = a.Owner.Email,
                    ActiveRoomCount = a.Rooms.Count(r => r.IsActive)
                };

                if (!string.IsNullOrEmpty(a.BusinessPermitS3Key))
                {
                    dto.BusinessPermitUrl = await _s3FileService.GetPresignedUrlAsync(a.BusinessPermitS3Key, presignExpiration);
                }

                if (!string.IsNullOrEmpty(a.DotAccreditationS3Key))
                {
                    dto.DotAccreditationUrl = await _s3FileService.GetPresignedUrlAsync(a.DotAccreditationS3Key, presignExpiration);
                }

                // BTC membership is a boolean flag and not a document

                results.Add(dto);
            }

            return Ok(results);
        }

        /// <summary>
        /// Create a new accommodation
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AccommodationResponseDto>> CreateAccommodation([FromForm] CreateAccommodationRequestDto request)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            // Verify user exists
            var user = await _context.Users.FindAsync(currentUserId.Value);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            string? logoUrl = null;

            // Handle logo upload if provided
            if (request.LogoFile != null)
            {
                var uploadResult = await _s3FileService.UploadFileAsync(request.LogoFile, "accommodation-logos");
                if (!uploadResult.Success)
                {
                    return BadRequest($"Logo upload failed: {uploadResult.Error}");
                }
                logoUrl = uploadResult.FileUrl;
            }

            var accommodation = new Accommodation
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                Logo = logoUrl,
                Address = request.Address,
                EmailAddress = request.EmailAddress,
                ContactNo = request.ContactNo,
                OwnerId = currentUserId.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Accommodations.Add(accommodation);
            await _context.SaveChangesAsync();

            // Reload with owner information
            await _context.Entry(accommodation)
                .Reference(a => a.Owner)
                .LoadAsync();

            var response = new AccommodationResponseDto
            {
                Id = accommodation.Id,
                Name = accommodation.Name,
                Description = accommodation.Description,
                Logo = accommodation.Logo,
                Address = accommodation.Address,
                EmailAddress = accommodation.EmailAddress,
                ContactNo = accommodation.ContactNo,
                IsActive = accommodation.IsActive,
                CreatedAt = accommodation.CreatedAt,
                UpdatedAt = accommodation.UpdatedAt,
                OwnerId = accommodation.OwnerId,
                OwnerName = accommodation.Owner!.FullName,
                OwnerEmail = accommodation.Owner.Email,
                ActiveRoomCount = 0
            };

            return CreatedAtAction(nameof(GetAccommodation), new { id = accommodation.Id }, response);
        }

        /// <summary>
        /// Upload a private business document (owner or admin)
        /// </summary>
        [HttpPost("{id}/documents/{docType}")]
        public async Task<IActionResult> UploadBusinessDocument(int id, string docType, IFormFile file)
        {
            var accommodation = await _context.Accommodations.FindAsync(id);
            if (accommodation == null) return NotFound("Accommodation not found");

            if (!CanModifyAccommodation(accommodation)) return Forbid();

            if (file == null || file.Length == 0) return BadRequest("No file provided");

            string folder = $"accommodations/{id}/documents";
            var uploadResult = await _s3FileService.UploadPrivateFileAsync(file, folder);
            if (!uploadResult.Success)
            {
                return BadRequest(new { Message = "Upload failed", Error = uploadResult.Error });
            }

            // Save S3 key to appropriate field
            var s3Key = uploadResult.S3Key;
            switch (docType.ToLower())
            {
                case "business-permit":
                    accommodation.BusinessPermitS3Key = s3Key;
                    break;
                case "dot-accreditation":
                    accommodation.DotAccreditationS3Key = s3Key;
                    break;
                default:
                    return BadRequest("Unknown document type");
            }

            accommodation.UpdateTimestamp();
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, S3Key = s3Key });
        }

        /// <summary>
        /// Delete a private business document (owner or admin)
        /// </summary>
        [HttpDelete("{id}/documents/{docType}")]
        public async Task<IActionResult> DeleteBusinessDocument(int id, string docType)
        {
            var accommodation = await _context.Accommodations.FindAsync(id);
            if (accommodation == null) return NotFound("Accommodation not found");

            if (!CanModifyAccommodation(accommodation)) return Forbid();

            string? currentKey = null;
            switch (docType.ToLower())
            {
                case "business-permit":
                    currentKey = accommodation.BusinessPermitS3Key;
                    accommodation.BusinessPermitS3Key = null;
                    break;
                case "dot-accreditation":
                    currentKey = accommodation.DotAccreditationS3Key;
                    accommodation.DotAccreditationS3Key = null;
                    break;
                default:
                    return BadRequest("Unknown document type");
            }

            if (!string.IsNullOrEmpty(currentKey))
            {
                await _s3FileService.DeleteFileAsync(currentKey);
            }

            accommodation.UpdateTimestamp();
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Update accommodation (owner or admin only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<AccommodationResponseDto>> UpdateAccommodation(int id, [FromForm] UpdateAccommodationRequestDto request)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.Owner)
                .Include(a => a.Rooms)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (accommodation == null)
            {
                return NotFound($"Accommodation with id {id} not found");
            }

            // Check if user can modify this accommodation
            if (!CanModifyAccommodation(accommodation))
            {
                return Forbid("You can only modify accommodations you own");
            }

            string? logoUrl = accommodation.Logo; // Keep existing logo by default

            // Handle new logo upload
            if (request.LogoFile != null)
            {
                try
                {
                    // Delete old logo if it exists
                    if (!string.IsNullOrEmpty(accommodation.Logo))
                    {
                        var oldS3Key = _s3FileService.ExtractS3KeyFromUrl(accommodation.Logo);
                        if (!string.IsNullOrEmpty(oldS3Key))
                        {
                            await _s3FileService.DeleteFileAsync(oldS3Key);
                        }
                    }

                    // Upload new logo
                    var uploadResult = await _s3FileService.UploadFileAsync(request.LogoFile, "accommodation-logos");
                    
                    if (!uploadResult.Success)
                    {
                        return BadRequest($"Failed to upload logo: {uploadResult.Error}");
                    }
                    
                    logoUrl = uploadResult.FileUrl;
                }
                catch (Exception ex)
                {
                    return BadRequest($"Failed to upload logo: {ex.Message}");
                }
            }

            // Update properties
            accommodation.Name = request.Name;
            accommodation.Description = request.Description ?? string.Empty;
            accommodation.Logo = logoUrl;
            accommodation.Address = request.Address;
            accommodation.EmailAddress = request.EmailAddress;
            accommodation.ContactNo = request.ContactNo;

            // Only admins may change BTC membership flag
            if (request.IsBtcMember.HasValue)
            {
                if (!IsAdmin())
                {
                    return Forbid("Only admins can change BTC membership status");
                }
                accommodation.IsBtcMember = request.IsBtcMember.Value;
            }

            accommodation.UpdateTimestamp();

            await _context.SaveChangesAsync();

            var response = new AccommodationResponseDto
            {
                Id = accommodation.Id,
                Name = accommodation.Name,
                Description = accommodation.Description,
                Logo = accommodation.Logo,
                Address = accommodation.Address,
                EmailAddress = accommodation.EmailAddress,
                ContactNo = accommodation.ContactNo,
                IsActive = accommodation.IsActive,
                CreatedAt = accommodation.CreatedAt,
                UpdatedAt = accommodation.UpdatedAt,
                OwnerId = accommodation.OwnerId,
                OwnerName = accommodation.Owner!.FullName,
                OwnerEmail = accommodation.Owner.Email,
                ActiveRoomCount = accommodation.Rooms.Count(r => r.IsActive)
            };

            return Ok(response);
        }

        /// <summary>
        /// Delete accommodation (soft delete - owner or admin only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAccommodation(int id)
        {
            var accommodation = await _context.Accommodations
                .FirstOrDefaultAsync(a => a.Id == id);

            if (accommodation == null)
            {
                return NotFound($"Accommodation with id {id} not found");
            }

            // Check if user can modify this accommodation
            if (!CanModifyAccommodation(accommodation))
            {
                return Forbid("You can only delete accommodations you own");
            }

            // Check if accommodation has active rooms
            var hasActiveRooms = await _context.Rooms
                .AnyAsync(r => r.AccommodationId == id && r.IsActive);

            if (hasActiveRooms)
            {
                return BadRequest("Cannot delete accommodation with active rooms. Please deactivate all rooms first.");
            }

            // Delete logo from S3 if it exists
            if (!string.IsNullOrEmpty(accommodation.Logo))
            {
                var s3Key = _s3FileService.ExtractS3KeyFromUrl(accommodation.Logo);
                if (!string.IsNullOrEmpty(s3Key))
                {
                    await _s3FileService.DeleteFileAsync(s3Key);
                }
            }

            // Soft delete
            accommodation.IsActive = false;
            accommodation.UpdateTimestamp();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id:int}/rooms")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAccommodationRooms(int id)
        {
            // Check if accommodation exists and user has access
            var accommodation = await _context.Accommodations
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();

            if (accommodation == null)
            {
                return NotFound(new { Message = "Accommodation not found." });
            }

            // Check authorization - user should be able to view rooms if they can access the accommodation
            // if (!CanModifyAccommodation(accommodation))
            // {
            //     return Forbid();
            // }

            // Get rooms belonging to this accommodation
            var rooms = await _context.Rooms
                .Where(r => r.AccommodationId == id && r.IsActive)
                .Select(r => new RoomListItemDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    DefaultPrice = r.DefaultPrice,
                    MaxGuests = r.MaxGuests,
                    IsActive = r.IsActive,
                    UpdatedAt = r.UpdatedAt,
                    MainPhotoUrl = r.Photos.OrderBy(p => p.Id).FirstOrDefault() != null 
                        ? r.Photos.OrderBy(p => p.Id).FirstOrDefault()!.S3Url 
                        : null,
                    PhotoCount = r.Photos.Count,
                    AmenityCount = r.RoomAmenities.Count,
                    MainAmenities = r.RoomAmenities
                        .OrderBy(ra => ra.Amenity.Name)
                        .Take(3)
                        .Select(ra => ra.Amenity.Name)
                        .ToList()
                })
                .OrderBy(r => r.Name)
                .ToListAsync();

            return Ok(rooms);
        }

        #region Private Methods

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        private bool CanModifyAccommodation(Accommodation accommodation)
        {
            var currentUserId = GetCurrentUserId();
            return currentUserId.HasValue && 
                   (accommodation.OwnerId == currentUserId.Value || IsAdmin());
        }

        #endregion

        
    }
}