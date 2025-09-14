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

        public RoomsController(
            IRoomService roomService, 
            ILogger<RoomsController> logger,
            ApplicationDbContext context,
            IS3FileService s3FileService)
        {
            _roomService = roomService;
            _logger = logger;
            _context = context;
            _s3FileService = s3FileService;
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

        #endregion
    }
}
