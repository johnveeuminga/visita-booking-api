using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using visita_booking_api.Services.Interfaces;

namespace VisitaBookingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IS3FileService _s3FileService;

        public UploadController(IS3FileService s3FileService)
        {
            _s3FileService = s3FileService;
        }

        /// <summary>
        /// Upload a single image (for featured events, etc.)
        /// </summary>
        [HttpPost("image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "No file provided" });
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { Message = "Invalid file type. Only JPG, PNG, and WEBP are allowed." });
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { Message = "File size exceeds 5MB limit" });
            }

            try
            {
                var uploadResult = await _s3FileService.UploadFileAsync(
                    file,
                    "featured-events"
                );

                if (!uploadResult.Success)
                {
                    return StatusCode(500, new { Message = "Upload failed", Error = uploadResult.Error });
                }

                return Ok(new
                {
                    s3Key = uploadResult.S3Key,
                    s3Url = uploadResult.FileUrl,
                    cdnUrl = uploadResult.FileUrl,
                    fileName = uploadResult.FileName,
                    contentType = uploadResult.ContentType,
                    size = uploadResult.FileSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Upload failed", Error = ex.Message });
            }
        }
    }
}
