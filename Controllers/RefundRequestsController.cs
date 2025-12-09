using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/bookings/{bookingId}/refund")]
    [Authorize]
    public class RefundRequestsController : ControllerBase
    {
        private readonly IRefundService _refundService;
        private readonly ILogger<RefundRequestsController> _logger;

        public RefundRequestsController(
            IRefundService refundService,
            ILogger<RefundRequestsController> logger
        )
        {
            _refundService = refundService;
            _logger = logger;
        }

        /// <summary>
        /// Check refund eligibility for a booking (preview)
        /// </summary>
        [HttpGet("eligibility")]
        public async Task<ActionResult<RefundEligibilityResponseDTO>> CheckEligibility(
            int bookingId
        )
        {
            try
            {
                var eligibility = await _refundService.CheckRefundEligibilityAsync(bookingId);
                return Ok(eligibility);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking refund eligibility for booking {BookingId}",
                    bookingId
                );
                return StatusCode(
                    500,
                    new { message = "An error occurred while checking refund eligibility" }
                );
            }
        }

        /// <summary>
        /// Request a refund for a booking
        /// </summary>
        [HttpPost("request")]
        public async Task<ActionResult<RefundRequestResponseDTO>> RequestRefund(
            int bookingId,
            [FromBody] CreateRefundRequestDTO request
        )
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var refundRequest = await _refundService.CreateRefundRequestAsync(
                    bookingId,
                    request,
                    userId
                );

                return CreatedAtAction(
                    nameof(GetRefundRequest),
                    new { bookingId, requestId = refundRequest.Id },
                    refundRequest
                );
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating refund request for booking {BookingId}",
                    bookingId
                );
                return StatusCode(
                    500,
                    new { message = "An error occurred while creating the refund request" }
                );
            }
        }

        /// <summary>
        /// Get refund requests for a booking
        /// </summary>
        [HttpGet("requests")]
        public async Task<ActionResult<List<RefundRequestResponseDTO>>> GetRefundRequests(
            int bookingId
        )
        {
            try
            {
                var requests = await _refundService.GetRefundRequestsByBookingIdAsync(bookingId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting refund requests for booking {BookingId}",
                    bookingId
                );
                return StatusCode(
                    500,
                    new { message = "An error occurred while retrieving refund requests" }
                );
            }
        }

        /// <summary>
        /// Get a specific refund request
        /// </summary>
        [HttpGet("requests/{requestId}")]
        public async Task<ActionResult<RefundRequestResponseDTO>> GetRefundRequest(
            int bookingId,
            int requestId
        )
        {
            try
            {
                var request = await _refundService.GetRefundRequestByIdAsync(requestId);

                if (request.BookingId != bookingId)
                    return NotFound(new { message = "Refund request not found for this booking" });

                return Ok(request);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund request {RequestId}", requestId);
                return StatusCode(
                    500,
                    new { message = "An error occurred while retrieving the refund request" }
                );
            }
        }
    }
}
