using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;

namespace visita_booking_api.Services.Implementation
{
    public class ParkService : IParkService
    {
        private readonly ApplicationDbContext _context;
        private readonly IS3FileService _fileUploadService;
        private readonly ILogger<ParkService> _logger;

        public ParkService(
            ApplicationDbContext context,
            IS3FileService fileUploadService,
            ILogger<ParkService> logger
        )
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<List<ParkDto>> GetAllParksAsync()
        {
            var parks = await _context
                .Parks.Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return parks.Select(MapToDto).ToList();
        }

        public async Task<List<ParkDto>> GetActiveParksAsync()
        {
            var parks = await _context
                .Parks.Include(p => p.Images)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return parks.Select(MapToDto).ToList();
        }

        public async Task<ParkDto?> GetParkByIdAsync(int id)
        {
            var park = await _context
                .Parks.Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            return park == null ? null : MapToDto(park);
        }

        public async Task<ParkDto> CreateParkAsync(CreateParkDto createDto, IFormFile? imageFile)
        {
            string? imageUrl = null;

            // Upload image if provided
            if (imageFile != null)
            {
                var uploadResult = await _fileUploadService.UploadFileAsync(imageFile, "parks");
                imageUrl = uploadResult.FileUrl; // Use FileUrl from FileUploadResponse
            }

            var park = new Park
            {
                Name = createDto.Name,
                Location = createDto.Location,
                Description = createDto.Description,
                Category = createDto.Category,
                ImageUrl = imageUrl,
                OpeningHours = createDto.OpeningHours,
                Price = createDto.Price,
                HasParking = createDto.HasParking,
                ParkingSlots = createDto.ParkingSlots,
                ParkingFee = createDto.ParkingFee,
                DisplayOrder = createDto.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Parks.Add(park);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Park created: {ParkName} (ID: {ParkId})", park.Name, park.Id);

            return MapToDto(park);
        }

        public async Task<ParkDto> UpdateParkAsync(
            int id,
            UpdateParkDto updateDto,
            IFormFile? imageFile
        )
        {
            var park = await _context.Parks.FindAsync(id);
            if (park == null)
            {
                throw new KeyNotFoundException($"Park with ID {id} not found");
            }

            // Upload new image if provided
            if (imageFile != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(park.ImageUrl))
                {
                    await _fileUploadService.DeleteFileAsync(park.ImageUrl);
                }

                var uploadResult = await _fileUploadService.UploadFileAsync(imageFile, "parks");
                park.ImageUrl = uploadResult.FileUrl; // Use FileUrl from FileUploadResponse
            }

            // Update properties
            park.Name = updateDto.Name;
            park.Location = updateDto.Location;
            park.Description = updateDto.Description;
            park.Category = updateDto.Category;
            park.OpeningHours = updateDto.OpeningHours;
            park.Price = updateDto.Price;
            park.HasParking = updateDto.HasParking;
            park.ParkingSlots = updateDto.ParkingSlots;
            park.ParkingFee = updateDto.ParkingFee;
            park.DisplayOrder = updateDto.DisplayOrder;
            park.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Park updated: {ParkName} (ID: {ParkId})", park.Name, park.Id);

            return MapToDto(park);
        }

        public async Task<bool> DeleteParkAsync(int id)
        {
            var park = await _context.Parks.FindAsync(id);
            if (park == null)
            {
                return false;
            }

            // Delete image if exists
            if (!string.IsNullOrEmpty(park.ImageUrl))
            {
                await _fileUploadService.DeleteFileAsync(park.ImageUrl);
            }

            _context.Parks.Remove(park);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Park deleted: {ParkName} (ID: {ParkId})", park.Name, park.Id);

            return true;
        }

        public async Task<ParkImageDto> AddParkImageAsync(
            int parkId,
            IFormFile imageFile,
            int displayOrder
        )
        {
            var park = await _context.Parks.FindAsync(parkId);
            if (park == null)
            {
                throw new KeyNotFoundException($"Park with ID {parkId} not found");
            }

            // Upload to S3 and get the URL
            var uploadResult = await _fileUploadService.UploadFileAsync(imageFile, "parks");

            // Validate upload result
            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.FileUrl))
            {
                _logger.LogError(
                    "S3 upload failed for park {ParkId}, upload result is null or empty",
                    parkId
                );
                throw new Exception(
                    "Failed to upload image to S3 - upload result is null or empty"
                );
            }

            _logger.LogInformation(
                "Successfully uploaded image to S3: {ImageUrl}",
                uploadResult.FileUrl
            );

            var parkImage = new ParkImage
            {
                ParkId = parkId,
                ImageUrl = uploadResult.FileUrl,
                DisplayOrder = displayOrder,
                CreatedAt = DateTime.UtcNow,
            };

            _context.ParkImages.Add(parkImage);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Image added to park {ParkName} (ID: {ParkId})",
                park.Name,
                parkId
            );

            return new ParkImageDto
            {
                Id = parkImage.Id,
                ParkId = parkImage.ParkId,
                ImageUrl = parkImage.ImageUrl,
                DisplayOrder = parkImage.DisplayOrder,
                CreatedAt = parkImage.CreatedAt,
            };
        }

        public async Task<bool> DeleteParkImageAsync(int parkId, int imageId)
        {
            var parkImage = await _context.ParkImages.FirstOrDefaultAsync(pi =>
                pi.Id == imageId && pi.ParkId == parkId
            );

            if (parkImage == null)
            {
                return false;
            }

            await _fileUploadService.DeleteFileAsync(parkImage.ImageUrl);

            _context.ParkImages.Remove(parkImage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Image {ImageId} deleted from park {ParkId}", imageId, parkId);

            return true;
        }

        public async Task<bool> UpdateParkImageOrderAsync(
            int parkId,
            int imageId,
            int newDisplayOrder
        )
        {
            var parkImage = await _context.ParkImages.FirstOrDefaultAsync(pi =>
                pi.Id == imageId && pi.ParkId == parkId
            );

            if (parkImage == null)
            {
                return false;
            }

            parkImage.DisplayOrder = newDisplayOrder;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Image {ImageId} display order updated to {DisplayOrder} for park {ParkId}",
                imageId,
                newDisplayOrder,
                parkId
            );

            return true;
        }

        private static ParkDto MapToDto(Park park)
        {
            return new ParkDto
            {
                Id = park.Id,
                Name = park.Name,
                Location = park.Location,
                Description = park.Description,
                Category = park.Category,
                ImageUrl = park.ImageUrl,
                OpeningHours = park.OpeningHours,
                Price = park.Price,
                HasParking = park.HasParking,
                ParkingSlots = park.ParkingSlots,
                ParkingFee = park.ParkingFee,
                DisplayOrder = park.DisplayOrder,
                IsActive = park.IsActive,
                CreatedAt = park.CreatedAt,
                UpdatedAt = park.UpdatedAt,
                Images = MapParkImages(park.Images),
            };
        }

        private static List<ParkImageDto> MapParkImages(ICollection<ParkImage>? images)
        {
            if (images == null || !images.Any())
                return new();

            return images
                .OrderBy(img => img.DisplayOrder)
                .Select(img => new ParkImageDto
                {
                    Id = img.Id,
                    ParkId = img.ParkId,
                    ImageUrl = img.ImageUrl,
                    DisplayOrder = img.DisplayOrder,
                    CreatedAt = img.CreatedAt,
                })
                .ToList();
        }
    }
}
