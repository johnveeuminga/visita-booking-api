using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ParksController : ControllerBase
    {
        private readonly IParkService _parkService;
        private readonly ILogger<ParksController> _logger;

        public ParksController(IParkService parkService, ILogger<ParksController> logger)
        {
            _parkService = parkService;
            _logger = logger;
        }

        // PUBLIC ENDPOINTS

        /// <summary>
        /// Get all active parks (public)
        /// </summary>
        [HttpGet("parks")]
        public async Task<ActionResult<List<ParkDto>>> GetActiveParks()
        {
            try
            {
                var parks = await _parkService.GetActiveParksAsync();
                return Ok(parks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active parks");
                return StatusCode(500, new { message = "An error occurred while fetching parks" });
            }
        }

        /// <summary>
        /// Get park by ID (public)
        /// </summary>
        [HttpGet("parks/{id}")]
        public async Task<ActionResult<ParkDto>> GetParkById(int id)
        {
            try
            {
                var park = await _parkService.GetParkByIdAsync(id);
                if (park == null)
                {
                    return NotFound(new { message = "Park not found" });
                }

                return Ok(park);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching park {ParkId}", id);
                return StatusCode(
                    500,
                    new { message = "An error occurred while fetching park details" }
                );
            }
        }

        // ADMIN ENDPOINTS

        /// <summary>
        /// Get all parks including inactive ones (admin only)
        /// </summary>
        [HttpGet("admin/parks")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<ParkDto>>> GetAllParks()
        {
            try
            {
                var parks = await _parkService.GetAllParksAsync();
                return Ok(parks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all parks");
                return StatusCode(500, new { message = "An error occurred while fetching parks" });
            }
        }

        /// <summary>
        /// Create a new park (admin only)
        /// </summary>
        [HttpPost("admin/parks")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkDto>> CreatePark(
            [FromForm] CreateParkDto createDto,
            [FromForm] IFormFile? imageFile
        )
        {
            try
            {
                var park = await _parkService.CreateParkAsync(createDto, imageFile);
                return CreatedAtAction(nameof(GetParkById), new { id = park.Id }, park);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating park");
                return StatusCode(
                    500,
                    new { message = "An error occurred while creating the park" }
                );
            }
        }

        /// <summary>
        /// Update an existing park (admin only)
        /// </summary>
        [HttpPut("admin/parks/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkDto>> UpdatePark(
            int id,
            [FromForm] UpdateParkDto updateDto,
            [FromForm] IFormFile? imageFile
        )
        {
            try
            {
                var park = await _parkService.UpdateParkAsync(id, updateDto, imageFile);
                return Ok(park);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Park not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating park {ParkId}", id);
                return StatusCode(
                    500,
                    new { message = "An error occurred while updating the park" }
                );
            }
        }

        /// <summary>
        /// Delete a park (admin only)
        /// </summary>
        [HttpDelete("admin/parks/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeletePark(int id)
        {
            try
            {
                var result = await _parkService.DeleteParkAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Park not found" });
                }

                return Ok(new { message = "Park deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting park {ParkId}", id);
                return StatusCode(
                    500,
                    new { message = "An error occurred while deleting the park" }
                );
            }
        }

        /// <summary>
        /// Toggle park active status (admin only)
        /// </summary>
        [HttpPatch("admin/parks/{id}/toggle-active")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleActive(int id)
        {
            try
            {
                var result = await _parkService.ToggleActiveAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Park not found" });
                }

                return Ok(new { message = "Park status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling park status {ParkId}", id);
                return StatusCode(
                    500,
                    new { message = "An error occurred while updating park status" }
                );
            }
        }
    }
}
