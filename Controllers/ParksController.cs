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
            [FromForm] IFormFile? imageFile,
            [FromForm] List<IFormFile>? additionalImages // ← ADD THIS PARAMETER
        )
        {
            try
            {
                var park = await _parkService.CreateParkAsync(createDto, imageFile);

                // Add additional images if provided
                if (additionalImages != null && additionalImages.Any())
                {
                    int displayOrder = 1;
                    foreach (var image in additionalImages)
                    {
                        await _parkService.AddParkImageAsync(park.Id, image, displayOrder++);
                    }
                }

                // Fetch the complete park with images
                var completePark = await _parkService.GetParkByIdAsync(park.Id);

                return CreatedAtAction(nameof(GetParkById), new { id = park.Id }, completePark); // ← RETURN completePark
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
            [FromForm] IFormFile? imageFile,
            [FromForm] List<IFormFile>? additionalImages // ← ADD THIS PARAMETER
        )
        {
            try
            {
                var park = await _parkService.UpdateParkAsync(id, updateDto, imageFile);

                // Add additional images if provided
                if (additionalImages != null && additionalImages.Any())
                {
                    // Get current max display order
                    var existingPark = await _parkService.GetParkByIdAsync(id);
                    int displayOrder =
                        existingPark?.Images.Any() == true
                            ? existingPark.Images.Max(img => img.DisplayOrder) + 1
                            : 1;

                    foreach (var image in additionalImages)
                    {
                        await _parkService.AddParkImageAsync(id, image, displayOrder++);
                    }
                }

                // Fetch the complete park with images
                var completePark = await _parkService.GetParkByIdAsync(id);

                return Ok(completePark); // ← RETURN completePark
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
        /// Add an image to a park (admin only)
        /// </summary>
        [HttpPost("admin/parks/{parkId}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkImageDto>> AddParkImage(
            int parkId,
            [FromForm] IFormFile imageFile,
            [FromForm] int displayOrder = 1
        )
        {
            try
            {
                if (imageFile == null)
                {
                    return BadRequest(new { message = "Image file is required" });
                }

                var parkImage = await _parkService.AddParkImageAsync(
                    parkId,
                    imageFile,
                    displayOrder
                );
                return Ok(parkImage);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Park not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding image to park {ParkId}", parkId);
                return StatusCode(
                    500,
                    new { message = "An error occurred while adding the image" }
                );
            }
        }

        /// <summary>
        /// Delete a park image (admin only)
        /// </summary>
        [HttpDelete("admin/parks/{parkId}/images/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteParkImage(int parkId, int imageId)
        {
            try
            {
                var result = await _parkService.DeleteParkImageAsync(parkId, imageId);
                if (!result)
                {
                    return NotFound(new { message = "Park image not found" });
                }

                return Ok(new { message = "Park image deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting park image {ImageId}", imageId);
                return StatusCode(
                    500,
                    new { message = "An error occurred while deleting the image" }
                );
            }
        }

        /// <summary>
        /// Update park image display order (admin only)
        /// </summary>
        [HttpPatch("admin/parks/{parkId}/images/{imageId}/order")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateParkImageOrder(
            int parkId,
            int imageId,
            [FromBody] UpdateImageOrderDto dto
        )
        {
            try
            {
                var result = await _parkService.UpdateParkImageOrderAsync(
                    parkId,
                    imageId,
                    dto.DisplayOrder
                );
                if (!result)
                {
                    return NotFound(new { message = "Park image not found" });
                }

                return Ok(new { message = "Image order updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating park image order {ImageId}", imageId);
                return StatusCode(
                    500,
                    new { message = "An error occurred while updating the image order" }
                );
            }
        }
    }
}
