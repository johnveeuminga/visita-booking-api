using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VisitaBookingApi.Data;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomsController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IS3FileService _s3FileService;
        private readonly IBookingService _bookingService;

        public RoomsController(
            IRoomService roomService, 
            ILogger<RoomsController> logger,
            ApplicationDbContext context,
            IS3FileService s3FileService,
            IBookingService bookingService)
        {
            _roomService = roomService;
            _logger = logger;
            _context = context;
            _s3FileService = s3FileService;
            _bookingService = bookingService;
        }

        /// <summary>
        /// Get all rooms
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<RoomListItemDTO>>> GetRooms([FromQuery] bool includeInactive = false)
        {
            try
            {
                var result = await _roomService.GetAllAsync(1, 50, includeInactive);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a specific room by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDetailsDTO>> GetRoom(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid room ID");
                }

                var room = await _roomService.GetByIdAsync(id);
                if (room == null)
                {
                    return NotFound($"Room with ID {id} not found");
                }

                return Ok(room);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room {RoomId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check room availability for specific dates and get pricing details
        /// </summary>
        /// <param name="id">Room ID</param>
        /// <param name="checkInDate">Check-in date</param>
        /// <param name="checkOutDate">Check-out date</param>
        /// <param name="numberOfGuests">Number of guests (optional, defaults to 1)</param>
        /// <returns>Availability status with pricing information</returns>
        [HttpGet("{id}/available")]
        [AllowAnonymous]
        public async Task<ActionResult<BookingAvailabilityResponseDto>> CheckRoomAvailability(
            int id,
            [FromQuery] DateTime checkInDate,
            [FromQuery] DateTime checkOutDate,
            [FromQuery] int numberOfGuests = 1)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid room ID");
                }

                // Validate dates
                if (checkInDate >= checkOutDate)
                {
                    return BadRequest("Check-out date must be after check-in date");
                }

                if (checkInDate.Date < DateTime.Now.Date)
                {
                    return BadRequest("Check-in date cannot be in the past");
                }

                if (numberOfGuests < 1 || numberOfGuests > 20)
                {
                    return BadRequest("Number of guests must be between 1 and 20");
                }

                // Verify room exists
                var room = await _context.Rooms
                    .Where(r => r.Id == id && r.IsActive)
                    .FirstOrDefaultAsync();

                if (room == null)
                {
                    return NotFound($"Room with ID {id} not found or inactive");
                }

                // Check if room can accommodate the number of guests
                if (numberOfGuests > room.MaxGuests)
                {
                    return BadRequest($"Room can only accommodate up to {room.MaxGuests} guests");
                }

                // Create availability request
                var availabilityRequest = new BookingAvailabilityRequestDto
                {
                    RoomId = id,
                    CheckInDate = checkInDate.Date,
                    CheckOutDate = checkOutDate.Date,
                    NumberOfGuests = numberOfGuests
                };

                // Check availability using the existing booking service
                var availability = await _bookingService.CheckAvailabilityAsync(availabilityRequest);

                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for room {RoomId} from {CheckIn} to {CheckOut}", 
                    id, checkInDate, checkOutDate);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Test endpoint to verify service integration
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Room service is working", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Create a new room (authorization required)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RoomDetailsDTO>> CreateRoom([FromForm] RoomCreateDTO request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized("User authentication required");
                }

                // Validate AccommodationId and user permission
                var accommodationValidation = await ValidateAccommodationAccess(request.AccommodationId);
                if (accommodationValidation != null)
                {
                    return accommodationValidation;
                }

                var result = await _roomService.CreateAsync(request);
                return CreatedAtAction(nameof(GetRoom), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                return BadRequest($"Failed to create room: {ex.Message}");
            }
        }

        /// <summary>
        /// Upload photos to an existing room
        /// </summary>
        [HttpPost("{id}/photos")]
        [Authorize]
        public async Task<ActionResult> UploadRoomPhotos(int id, [FromForm] List<IFormFile> photos)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized("User authentication required");
                }

                if (id <= 0)
                {
                    return BadRequest("Invalid room ID");
                }

                if (photos == null || !photos.Any())
                {
                    return BadRequest("No photos provided");
                }

                // Validate room exists and user has access
                var room = await _context.Rooms
                    .Include(r => r.Accommodation)
                    .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

                if (room == null)
                {
                    return NotFound($"Room with ID {id} not found");
                }

                // Check authorization - user should own the accommodation or be admin
                if (room.Accommodation != null)
                {
                    var canModify = currentUserId.Value == room.Accommodation.OwnerId || IsAdmin();
                    if (!canModify)
                    {
                        return Forbid("You can only upload photos for rooms you own");
                    }
                }

                // Get current max display order for proper sequencing
                var maxDisplayOrder = await _context.RoomPhotos
                    .Where(p => p.RoomId == id && p.IsActive)
                    .MaxAsync(p => (int?)p.DisplayOrder) ?? -1;

                var uploadedPhotos = new List<RoomPhotoDTO>();
                var errors = new List<object>();

                foreach (var photo in photos)
                {
                    try
                    {
                        // Validate file
                        if (photo.Length == 0)
                        {
                            errors.Add(new { fileName = photo.FileName, error = "File is empty" });
                            continue;
                        }

                        if (photo.Length > 10 * 1024 * 1024) // 10MB limit
                        {
                            errors.Add(new { fileName = photo.FileName, error = "File size exceeds 10MB limit" });
                            continue;
                        }

                        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                        if (!allowedTypes.Contains(photo.ContentType?.ToLower()))
                        {
                            errors.Add(new { fileName = photo.FileName, error = "Invalid file type. Only JPEG, PNG, and WebP are allowed" });
                            continue;
                        }

                        // Upload to S3
                        var uploadResult = await _s3FileService.UploadFileAsync(photo, $"rooms/{id}/photos");
                        if (!uploadResult.Success)
                        {
                            errors.Add(new { fileName = photo.FileName, error = uploadResult.Error });
                            continue;
                        }

                        // Create room photo record
                        var roomPhoto = new RoomPhoto
                        {
                            RoomId = id,
                            S3Key = uploadResult.S3Key ?? "",
                            S3Url = uploadResult.FileUrl ?? "",
                            FileName = uploadResult.FileName ?? photo.FileName,
                            FileSize = uploadResult.FileSize,
                            ContentType = uploadResult.ContentType ?? photo.ContentType ?? "image/jpeg",
                            DisplayOrder = ++maxDisplayOrder,
                            IsActive = true,
                            UploadedAt = DateTime.UtcNow,
                            LastModified = DateTime.UtcNow
                        };

                        _context.RoomPhotos.Add(roomPhoto);
                        await _context.SaveChangesAsync();

                        // Map to DTO for response
                        var photoDto = new RoomPhotoDTO
                        {
                            Id = roomPhoto.Id,
                            FileName = roomPhoto.FileName,
                            FileUrl = roomPhoto.CdnUrl ?? roomPhoto.S3Url,
                            LastModified = roomPhoto.LastModified
                        };

                        uploadedPhotos.Add(photoDto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading photo {FileName} for room {RoomId}", photo.FileName, id);
                        errors.Add(new { fileName = photo.FileName, error = "Upload failed due to server error" });
                    }
                }

                // Update room timestamp
                room.UpdateTimestamp();
                await _context.SaveChangesAsync();

                // Return response with uploaded photos and any errors
                return Ok(new
                {
                    message = $"Successfully uploaded {uploadedPhotos.Count} of {photos.Count} photos",
                    uploadedPhotos,
                    errors,
                    totalPhotos = await _context.RoomPhotos.CountAsync(p => p.RoomId == id && p.IsActive)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photos for room {RoomId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an existing room (authorization required)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<RoomDetailsDTO>> UpdateRoom(int id, [FromForm] RoomUpdateDTO request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized("User authentication required");
                }

                if (id <= 0)
                {
                    return BadRequest("Invalid room ID");
                }

                var result = await _roomService.UpdateAsync(id, request);
                if (result == null)
                {
                    return NotFound($"Room with ID {id} not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room {RoomId}", id);
                return BadRequest($"Failed to update room: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a room (authorization required)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteRoom(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized("User authentication required");
                }

                if (id <= 0)
                {
                    return BadRequest("Invalid room ID");
                }

                var result = await _roomService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Room with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room {RoomId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Toggle room active status (authorization required)
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize]
        public async Task<ActionResult> ToggleRoomStatus(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized("User authentication required");
                }

                if (id <= 0)
                {
                    return BadRequest("Invalid room ID");
                }

                var result = await _roomService.ToggleActiveStatusAsync(id);
                if (!result)
                {
                    return NotFound($"Room with ID {id} not found");
                }

                return Ok(new { message = "Room status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling room status {RoomId}", id);
                return StatusCode(500, "Internal server error");
            }
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

        private async Task<ActionResult?> ValidateAccommodationAccess(int accommodationId)
        {
            // Check if accommodation exists and is active
            var accommodation = await _context.Accommodations
                .Where(a => a.Id == accommodationId && a.IsActive)
                .FirstOrDefaultAsync();

            if (accommodation == null)
            {
                return NotFound(new { Message = "Accommodation not found or inactive" });
            }

            // Check if user can modify this accommodation
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue || 
                (accommodation.OwnerId != currentUserId.Value && !IsAdmin()))
            {
                return Forbid("You can only create rooms for accommodations you own");
            }

            return null; // Validation passed
        }

        #endregion
    }
}
