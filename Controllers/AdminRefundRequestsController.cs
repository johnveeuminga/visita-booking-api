using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/admin/refund-requests")]
    [Authorize(Roles = "Admin")]
    public class AdminRefundRequestsController : ControllerBase
    {
        private readonly IRefundService _refundService;
        private readonly ILogger<AdminRefundRequestsController> _logger;

        public AdminRefundRequestsController(
            IRefundService refundService,
            ILogger<AdminRefundRequestsController> logger
        )
        {
            _refundService = refundService;
            _logger = logger;
        }

        /// <summary>
        /// Get all pending refund requests (paginated)
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<List<RefundRequestResponseDTO>>> GetPendingRequests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            try
            {
                if (page < 1 || pageSize < 1 || pageSize > 100)
                    return BadRequest(new { message = "Invalid pagination parameters" });

                var requests = await _refundService.GetPendingRefundRequestsAsync(page, pageSize);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending refund requests");
                return StatusCode(
                    500,
                    new { message = "An error occurred while retrieving pending refund requests" }
                );
            }
        }

        /// <summary>
        /// Get a specific refund request by ID
        /// </summary>
        [HttpGet("{requestId}")]
        public async Task<ActionResult<RefundRequestResponseDTO>> GetRefundRequest(int requestId)
        {
            try
            {
                var request = await _refundService.GetRefundRequestByIdAsync(requestId);
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

        /// <summary>
        /// Approve or reject a refund request
        /// </summary>
        [HttpPost("{requestId}/process")]
        public async Task<ActionResult<RefundRequestResponseDTO>> ProcessRefundRequest(
            int requestId,
            [FromBody] ProcessRefundRequestDTO request
        )
        {
            try
            {
                var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var processedRequest = await _refundService.ProcessRefundRequestAsync(
                    requestId,
                    request,
                    adminUserId
                );

                return Ok(processedRequest);
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
                _logger.LogError(ex, "Error processing refund request {RequestId}", requestId);
                return StatusCode(
                    500,
                    new { message = "An error occurred while processing the refund request" }
                );
            }
        }
    }
}
