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
                    IsActive = a.IsActive,
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
                IsActive = accommodation.IsActive,
                CreatedAt = accommodation.CreatedAt,
                UpdatedAt = accommodation.UpdatedAt,
                OwnerId = accommodation.OwnerId,
                OwnerName = accommodation.Owner?.FullName ?? "Unknown",
                OwnerEmail = accommodation.Owner?.Email ?? "Unknown",
                ActiveRoomCount = accommodation.Rooms.Count(r => r.IsActive)
            };

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
                .Select(a => new AccommodationResponseDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Logo = a.Logo,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    OwnerId = a.OwnerId,
                    OwnerName = a.Owner!.FullName,
                    OwnerEmail = a.Owner.Email,
                    ActiveRoomCount = a.Rooms.Count(r => r.IsActive)
                })
                .OrderBy(a => a.Name)
                .ToListAsync();

            return Ok(accommodations);
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
                Description = request.Description,
                Logo = logoUrl,
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
            accommodation.Logo = logoUrl;
            accommodation.UpdateTimestamp();

            await _context.SaveChangesAsync();

            var response = new AccommodationResponseDto
            {
                Id = accommodation.Id,
                Name = accommodation.Name,
                Description = accommodation.Description,
                Logo = accommodation.Logo,
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