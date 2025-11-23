using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using visita_booking_api.Models.Entities;
using visita_booking_api.Models.Enums;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/owner/establishments")]
    [Authorize(Roles = "Hotel")]
    public class OwnerEstablishmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IS3FileService _s3FileService;

        public OwnerEstablishmentsController(
            ApplicationDbContext context,
            IS3FileService s3FileService
        )
        {
            _context = context;
            _s3FileService = s3FileService;
        }

        /// <summary>
        /// Owner: Get all my establishments
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyEstablishments()
        {
            var ownerId = GetCurrentUserId();
            if (!ownerId.HasValue)
                return Unauthorized();

            var establishments = await _context
                .Establishments.Where(e => e.OwnerId == ownerId.Value)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.Logo,
                    Category = e.Category.ToString(),
                    Status = e.Status.ToString(),
                    e.IsActive,
                    e.RejectionReason,
                    e.CreatedAt,
                    e.UpdatedAt,
                })
                .ToListAsync();

            return Ok(new { success = true, data = establishments });
        }

        /// <summary>
        /// Owner: Get single establishment details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEstablishment(int id)
        {
            var ownerId = GetCurrentUserId();
            if (!ownerId.HasValue)
                return Unauthorized();

            var establishment = await _context
                .Establishments.Include(e => e.Hours)
                .Where(e => e.Id == id && e.OwnerId == ownerId.Value)
                .FirstOrDefaultAsync();

            if (establishment == null)
                return NotFound(new { success = false, message = "Establishment not found" });

            var response = new
            {
                establishment.Id,
                establishment.Name,
                establishment.Description,
                establishment.Logo,
                establishment.CoverImage,
                Category = establishment.Category.ToString(),
                establishment.Address,
                establishment.City,
                establishment.Latitude,
                establishment.Longitude,
                establishment.ContactNumber,
                establishment.Email,
                establishment.Website,
                establishment.FacebookPage,
                Status = establishment.Status.ToString(),
                establishment.IsActive,
                establishment.RejectionReason,
                establishment.CreatedAt,
                establishment.UpdatedAt,
                Hours = establishment.Hours?.Select(h => new
                {
                    h.Id,
                    DayOfWeek = h.DayOfWeek.ToString(),
                    OpenTime = h.OpenTime?.ToString(@"hh\:mm"),
                    CloseTime = h.CloseTime?.ToString(@"hh\:mm"),
                    h.IsClosed,
                }),
            };

            // Get presigned URL for business permit if exists
            string? businessPermitUrl = null;
            if (!string.IsNullOrEmpty(establishment.BusinessPermitS3Key))
            {
                businessPermitUrl = await _s3FileService.GetPresignedUrlAsync(
                    establishment.BusinessPermitS3Key,
                    TimeSpan.FromMinutes(15)
                );
            }

            return Ok(
                new
                {
                    success = true,
                    data = response,
                    businessPermitUrl,
                }
            );
        }

        /// <summary>
        /// Owner: Create new establishment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateEstablishmentRequest request)
        {
            var ownerId = GetCurrentUserId();
            if (!ownerId.HasValue)
                return Unauthorized();

            // Validate category
            if (!Enum.TryParse<EstablishmentCategory>(request.Category, true, out var category))
                return BadRequest(new { success = false, message = "Invalid category" });

            var establishment = new Establishment
            {
                Name = request.Name,
                Description = request.Description,
                Category = category,
                Address = request.Address,
                City = request.City ?? "Baguio",
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ContactNumber = request.ContactNumber,
                Email = request.Email,
                Website = request.Website,
                FacebookPage = request.FacebookPage,
                OwnerId = ownerId.Value,
                Status = EstablishmentStatus.Pending,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            // Upload logo if provided
            if (request.Logo != null)
            {
                var uploadResult = await _s3FileService.UploadFileAsync(
                    request.Logo,
                    "establishment-logos"
                );
                if (!uploadResult.Success)
                {
                    return BadRequest(
                        new
                        {
                            success = false,
                            message = $"Logo upload failed: {uploadResult.Error}",
                        }
                    );
                }
                establishment.Logo = uploadResult.FileUrl;
            }
            // Upload business permit
            if (request.BusinessPermit != null)
            {
                var uploadResult = await _s3FileService.UploadPrivateFileAsync(
                    request.BusinessPermit,
                    $"establishments/{ownerId.Value}/permits"
                );
                if (!uploadResult.Success)
                {
                    return BadRequest(
                        new
                        {
                            success = false,
                            message = $"Business permit upload failed: {uploadResult.Error}",
                        }
                    );
                }
                establishment.BusinessPermitS3Key = uploadResult.S3Key;
            }

            _context.Establishments.Add(establishment);
            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    success = true,
                    message = "Establishment created and pending admin approval",
                    data = new
                    {
                        establishment.Id,
                        establishment.Name,
                        Status = establishment.Status.ToString(),
                    },
                }
            );
        }

        /// <summary>
        /// Owner: Update establishment
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] UpdateEstablishmentRequest request
        )
        {
            var ownerId = GetCurrentUserId();
            if (!ownerId.HasValue)
                return Unauthorized();

            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == id && e.OwnerId == ownerId.Value
            );

            if (establishment == null)
                return NotFound(new { success = false, message = "Establishment not found" });

            // Update basic info
            if (!string.IsNullOrEmpty(request.Name))
                establishment.Name = request.Name;
            if (request.Description != null)
                establishment.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Address))
                establishment.Address = request.Address;
            if (!string.IsNullOrEmpty(request.City))
                establishment.City = request.City;
            if (request.Latitude.HasValue)
                establishment.Latitude = request.Latitude;
            if (request.Longitude.HasValue)
                establishment.Longitude = request.Longitude;
            if (request.ContactNumber != null)
                establishment.ContactNumber = request.ContactNumber;
            if (request.Email != null)
                establishment.Email = request.Email;
            if (request.Website != null)
                establishment.Website = request.Website;
            if (request.FacebookPage != null)
                establishment.FacebookPage = request.FacebookPage;

            // Upload logo if provided
            if (request.Logo != null)
            {
                var uploadResult = await _s3FileService.UploadFileAsync(
                    request.Logo,
                    "establishment-logos"
                );
                if (!uploadResult.Success)
                {
                    return BadRequest(
                        new
                        {
                            success = false,
                            message = $"Logo upload failed: {uploadResult.Error}",
                        }
                    );
                }
                establishment.Logo = uploadResult.FileUrl;
            }
            establishment.UpdateTimestamp();
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Establishment updated successfully" });
        }

        /// <summary>
        /// Owner: Delete (deactivate) establishment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ownerId = GetCurrentUserId();
            if (!ownerId.HasValue)
                return Unauthorized();

            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == id && e.OwnerId == ownerId.Value
            );

            if (establishment == null)
                return NotFound(new { success = false, message = "Establishment not found" });

            establishment.IsActive = false;
            establishment.UpdateTimestamp();
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Establishment deactivated" });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    // Request DTOs
    public class CreateEstablishmentRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? City { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? FacebookPage { get; set; }
        public IFormFile? Logo { get; set; }
        public IFormFile? BusinessPermit { get; set; }
    }

    public class UpdateEstablishmentRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? FacebookPage { get; set; }
        public IFormFile? Logo { get; set; }
    }
}
