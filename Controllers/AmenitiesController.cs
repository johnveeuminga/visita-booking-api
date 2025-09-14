using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.DTOs;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AmenitiesController : ControllerBase
    {
        private readonly IAmenityService _amenityService;
        private readonly ILogger<AmenitiesController> _logger;

        public AmenitiesController(IAmenityService amenityService, ILogger<AmenitiesController> logger)
        {
            _amenityService = amenityService;
            _logger = logger;
        }

        /// <summary>
        /// Get all amenities 
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<AmenityDTO>>> GetAmenities([FromQuery] bool includeInactive = false)
        {
            try
            {
                var amenities = await _amenityService.GetAllAsync(includeInactive);
                return Ok(amenities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving amenities");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Test endpoint to verify service integration
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Amenity service is working", timestamp = DateTime.UtcNow });
        }
    }
}
