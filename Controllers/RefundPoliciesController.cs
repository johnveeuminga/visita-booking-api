using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/accommodations/{accommodationId}/refund-policy")]
    [Authorize(Roles = "Hotel")]
    public class RefundPoliciesController : ControllerBase
    {
        private readonly IRefundService _refundService;
        private readonly ILogger<RefundPoliciesController> _logger;

        public RefundPoliciesController(
            IRefundService refundService,
            ILogger<RefundPoliciesController> logger
        )
        {
            _refundService = refundService;
            _logger = logger;
        }

        /// <summary>
        /// Get refund policy for an accommodation
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<RefundPolicyResponseDTO>> GetPolicy(int accommodationId)
        {
            try
            {
                var policy = await _refundService.GetPolicyByAccommodationIdAsync(accommodationId);

                if (policy == null)
                    return NotFound(
                        new { message = "No refund policy found for this accommodation" }
                    );

                return Ok(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting refund policy for accommodation {AccommodationId}",
                    accommodationId
                );
                return StatusCode(
                    500,
                    new { message = "An error occurred while retrieving the refund policy" }
                );
            }
        }

        /// <summary>
        /// Create refund policy for an accommodation
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RefundPolicyResponseDTO>> CreatePolicy(
            int accommodationId,
            [FromBody] CreateRefundPolicyRequestDTO request
        )
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var policy = await _refundService.CreatePolicyAsync(
                    accommodationId,
                    request,
                    userId
                );

                return CreatedAtAction(nameof(GetPolicy), new { accommodationId }, policy);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating refund policy for accommodation {AccommodationId}",
                    accommodationId
                );
                return StatusCode(
                    500,
                    new { message = "An error occurred while creating the refund policy" }
                );
            }
        }

        /// <summary>
        /// Update refund policy for an accommodation
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<RefundPolicyResponseDTO>> UpdatePolicy(
            int accommodationId,
            [FromBody] UpdateRefundPolicyRequestDTO request
        )
        {
            try
            {
                var policy = await _refundService.UpdatePolicyAsync(accommodationId, request);
                return Ok(policy);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating refund policy for accommodation {AccommodationId}",
                    accommodationId
                );
                return StatusCode(
                    500,
                    new { message = "An error occurred while updating the refund policy" }
                );
            }
        }

        /// <summary>
        /// Delete (deactivate) refund policy for an accommodation
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeletePolicy(int accommodationId)
        {
            try
            {
                var result = await _refundService.DeletePolicyAsync(accommodationId);

                if (!result)
                    return NotFound(
                        new { message = "No active refund policy found for this accommodation" }
                    );

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting refund policy for accommodation {AccommodationId}",
                    accommodationId
                );
                return StatusCode(
                    500,
                    new { message = "An error occurred while deleting the refund policy" }
                );
            }
        }

        /// <summary>
        /// Get predefined refund policy templates
        /// </summary>
        [HttpGet("~/api/refund-policies/templates")]
        [AllowAnonymous]
        public async Task<ActionResult<List<PredefinedPolicyDTO>>> GetPredefinedPolicies()
        {
            try
            {
                var policies = await _refundService.GetPredefinedPoliciesAsync();
                return Ok(policies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting predefined refund policies");
                return StatusCode(
                    500,
                    new { message = "An error occurred while retrieving policy templates" }
                );
            }
        }
    }
}
