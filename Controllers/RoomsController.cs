using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.DTOs;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(IRoomService roomService, ILogger<RoomsController> logger)
        {
            _roomService = roomService;
            _logger = logger;
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
    }
}
