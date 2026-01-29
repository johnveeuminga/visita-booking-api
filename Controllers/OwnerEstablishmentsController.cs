using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VisitaBookingAPI.DTOs;
using VisitaBookingAPI.Services.Interfaces;

namespace VisitaBookingAPI.Controllers
{
    [ApiController]
    [Route("api/owner/establishments")]
    [Authorize]
    public class OwnerEstablishmentsController : ControllerBase
    {
        private readonly IEstablishmentService _establishmentService;

        public OwnerEstablishmentsController(IEstablishmentService establishmentService)
        {
            _establishmentService = establishmentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EstablishmentListDto>>> GetMyEstablishments()
        {
            var userId = GetCurrentUserId();
            var establishments = await _establishmentService.GetMyEstablishmentsAsync(userId);
            return Ok(establishments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EstablishmentDetailDto>> GetEstablishment(int id)
        {
            var userId = GetCurrentUserId();
            var establishment = await _establishmentService.GetEstablishmentByIdAsync(id);
            if (establishment == null || establishment.Owner.Id != userId)
            {
                return NotFound(new { message = "Establishment not found or access denied" });
            }
            return Ok(establishment);
        }

        [HttpPost]
        public async Task<ActionResult<EstablishmentDetailDto>> CreateEstablishment(
            [FromBody] CreateEstablishmentDto dto
        )
        {
            try
            {
                var userId = GetCurrentUserId();
                var establishment = await _establishmentService.CreateEstablishmentAsync(
                    userId,
                    dto
                );
                return CreatedAtAction(
                    nameof(GetEstablishment),
                    new { id = establishment.Id },
                    establishment
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EstablishmentDetailDto>> UpdateEstablishment(
            int id,
            [FromBody] UpdateEstablishmentDto dto
        )
        {
            try
            {
                var userId = GetCurrentUserId();
                var establishment = await _establishmentService.UpdateEstablishmentAsync(
                    id,
                    userId,
                    dto
                );
                return Ok(establishment);
            }
            catch (UnauthorizedAccessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEstablishment(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _establishmentService.DeleteEstablishmentAsync(id, userId);
            if (!success)
            {
                return NotFound(new { message = "Establishment not found or access denied" });
            }
            return NoContent();
        }

        [HttpPost("{id}/business-permit")]
        public async Task<ActionResult> UploadBusinessPermit(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "File is required" });
            }

            try
            {
                var userId = GetCurrentUserId();
                var url = await _establishmentService.UploadBusinessPermitAsync(id, userId, file);
                return Ok(new { url, message = "Business permit uploaded successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/logo")]
        public async Task<ActionResult> UploadLogo(int id, IFormFile file)
        {
            if (file == null || file.Length == 0 || !file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new { message = "Valid image file is required" });
            }

            try
            {
                var userId = GetCurrentUserId();
                var url = await _establishmentService.UploadLogoAsync(id, userId, file);
                return Ok(new { url, message = "Logo uploaded successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cover")]
        public async Task<ActionResult> UploadCoverImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0 || !file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new { message = "Valid image file is required" });
            }

            try
            {
                var userId = GetCurrentUserId();
                var url = await _establishmentService.UploadCoverImageAsync(id, userId, file);
                return Ok(new { url, message = "Cover image uploaded successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/images")]
        public async Task<ActionResult<EstablishmentImageDto>> AddImage(
            int id,
            IFormFile file,
            [FromForm] string? caption = null,
            [FromForm] int displayOrder = 0
        )
        {
            if (file == null || file.Length == 0 || !file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new { message = "Valid image file is required" });
            }

            try
            {
                var userId = GetCurrentUserId();
                var image = await _establishmentService.AddEstablishmentImageAsync(
                    id,
                    userId,
                    file,
                    caption,
                    displayOrder
                );
                return Ok(image);
            }
            catch (UnauthorizedAccessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}/images/{imageId}")]
        public async Task<ActionResult> DeleteImage(int id, int imageId)
        {
            var userId = GetCurrentUserId();
            var success = await _establishmentService.DeleteEstablishmentImageAsync(
                id,
                imageId,
                userId
            );
            if (!success)
            {
                return NotFound(new { message = "Image not found or access denied" });
            }
            return NoContent();
        }

        [HttpPost("{id}/menu-items")]
        public async Task<ActionResult<EstablishmentMenuItemDto>> AddMenuItem(
            int id,
            [FromBody] CreateMenuItemDto dto
        )
        {
            try
            {
                var userId = GetCurrentUserId();
                var menuItem = await _establishmentService.AddMenuItemAsync(id, userId, dto);
                return CreatedAtAction(nameof(GetEstablishment), new { id }, menuItem);
            }
            catch (UnauthorizedAccessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/menu-items/{menuItemId}")]
        public async Task<ActionResult<EstablishmentMenuItemDto>> UpdateMenuItem(
            int id,
            int menuItemId,
            [FromBody] UpdateMenuItemDto dto
        )
        {
            try
            {
                var userId = GetCurrentUserId();
                var menuItem = await _establishmentService.UpdateMenuItemAsync(
                    id,
                    menuItemId,
                    userId,
                    dto
                );
                return Ok(menuItem);
            }
            catch (UnauthorizedAccessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}/menu-items/{menuItemId}")]
        public async Task<ActionResult> DeleteMenuItem(int id, int menuItemId)
        {
            var userId = GetCurrentUserId();
            var success = await _establishmentService.DeleteMenuItemAsync(id, menuItemId, userId);
            if (!success)
            {
                return NotFound(new { message = "Menu item not found or access denied" });
            }
            return NoContent();
        }

        [HttpPost("{id}/menu-items/{menuItemId}/image")]
        public async Task<ActionResult> UploadMenuItemImage(int id, int menuItemId, IFormFile file)
        {
            if (file == null || file.Length == 0 || !file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new { message = "Valid image file is required" });
            }

            try
            {
                var userId = GetCurrentUserId();
                var url = await _establishmentService.UploadMenuItemImageAsync(
                    id,
                    menuItemId,
                    userId,
                    file
                );
                return Ok(new { url, message = "Menu item image uploaded successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return int.Parse(userIdClaim);
        }
    }
}
