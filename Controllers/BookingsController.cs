using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(
            IBookingService bookingService,
            ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Check room availability for specified dates
        /// </summary>
        /// <param name="request">Availability request</param>
        /// <returns>Availability information with pricing</returns>
        [HttpPost("availability")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckAvailability([FromBody] BookingAvailabilityRequestDto request)
        {
            try
            {
                var availability = await _bookingService.CheckAvailabilityAsync(request);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for room {RoomId}", request.RoomId);
                return StatusCode(500, new { message = "An error occurred while checking availability" });
            }
        }

        /// <summary>
        /// Create a new booking reservation
        /// </summary>
        /// <param name="request">Booking creation request</param>
        /// <returns>Created booking with reservation details</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto request)
        {
            try
            {
                var userId = GetUserId();
                var booking = await _bookingService.CreateBookingAsync(request, userId);
                
                return CreatedAtAction(
                    nameof(GetBooking),
                    new { id = booking.Id },
                    booking
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking for user {UserId}", GetUserId());
                return StatusCode(500, new { message = "An error occurred while creating the booking" });
            }
        }

        /// <summary>
        /// Get booking details by ID
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Booking details</returns>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetBooking(int id)
        {
            try
            {
                var userId = IsAdmin() ? (int?)null : GetUserId();
                var booking = await _bookingService.GetBookingAsync(id, userId);
                
                if (booking == null)
                    return NotFound(new { message = "Booking not found" });

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the booking" });
            }
        }

        /// <summary>
        /// Get booking details by booking reference
        /// </summary>
        /// <param name="reference">Booking reference</param>
        /// <returns>Booking details</returns>
        [HttpGet("reference/{reference}")]
        [Authorize]
        public async Task<IActionResult> GetBookingByReference(string reference)
        {
            try
            {
                var userId = IsAdmin() ? (int?)null : GetUserId();
                var booking = await _bookingService.GetBookingByReferenceAsync(reference, userId);
                
                if (booking == null)
                    return NotFound(new { message = "Booking not found" });

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking by reference {Reference}", reference);
                return StatusCode(500, new { message = "An error occurred while retrieving the booking" });
            }
        }

        /// <summary>
        /// Search bookings with filters and pagination
        /// </summary>
        /// <param name="request">Search criteria</param>
        /// <returns>Paginated search results</returns>
        [HttpPost("search")]
        [Authorize]
        public async Task<IActionResult> SearchBookings([FromBody] BookingSearchRequestDto request)
        {
            try
            {
                var userId = IsAdmin() ? (int?)null : GetUserId();
                var results = await _bookingService.SearchBookingsAsync(request, userId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching bookings");
                return StatusCode(500, new { message = "An error occurred while searching bookings" });
            }
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">Cancellation request</param>
        /// <returns>Cancelled booking details</returns>
        [HttpPost("{id:int}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingRequestDto request)
        {
            try
            {
                var userId = GetUserId();
                request.CancelledBy = GetUserEmail();
                
                var booking = await _bookingService.CancelBookingAsync(id, request, userId);
                return Ok(booking);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
                return StatusCode(500, new { message = "An error occurred while cancelling the booking" });
            }
        }

        /// <summary>
        /// Extend a reservation
        /// </summary>
        /// <param name="request">Extension request</param>
        /// <returns>Extended reservation details</returns>
        [HttpPost("reservations/extend")]
        [Authorize]
        public async Task<IActionResult> ExtendReservation([FromBody] ExtendReservationRequestDto request)
        {
            try
            {
                var userId = GetUserId();
                var reservation = await _bookingService.ExtendReservationAsync(request, userId);
                return Ok(reservation);
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
                _logger.LogError(ex, "Error extending reservation {Reference}", request.ReservationReference);
                return StatusCode(500, new { message = "An error occurred while extending the reservation" });
            }
        }

        /// <summary>
        /// Get user's booking history
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paginated booking history</returns>
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetBookingHistory(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetUserId();
                var history = await _bookingService.GetUserBookingHistoryAsync(userId, pageNumber, pageSize);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking history for user {UserId}", GetUserId());
                return StatusCode(500, new { message = "An error occurred while retrieving booking history" });
            }
        }

        /// <summary>
        /// Get upcoming bookings for the current user
        /// </summary>
        /// <returns>List of upcoming bookings</returns>
        [HttpGet("upcoming")]
        [Authorize]
        public async Task<IActionResult> GetUpcomingBookings()
        {
            try
            {
                var userId = GetUserId();
                var upcomingBookings = await _bookingService.GetUpcomingBookingsAsync(userId);
                return Ok(upcomingBookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming bookings for user {UserId}", GetUserId());
                return StatusCode(500, new { message = "An error occurred while retrieving upcoming bookings" });
            }
        }

        /// <summary>
        /// Update booking status (Admin only)
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">Status update request</param>
        /// <returns>Updated booking details</returns>
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto request)
        {
            try
            {
                request.UpdatedBy = GetUserEmail();
                var booking = await _bookingService.UpdateBookingStatusAsync(id, request);
                return Ok(booking);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status for {BookingId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the booking status" });
            }
        }

        /// <summary>
        /// Process refund for a booking (Admin only)
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">Refund request</param>
        /// <returns>Refund processing result</returns>
        [HttpPost("{id:int}/refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessRefund(int id, [FromBody] RefundPaymentRequestDto request)
        {
            try
            {
                // Set booking reference from route parameter
                var booking = await _bookingService.GetBookingAsync(id);
                if (booking == null)
                    return NotFound(new { message = "Booking not found" });

                request.BookingReference = booking.BookingReference;
                request.ProcessedBy = GetUserEmail();

                var success = await _bookingService.ProcessRefundAsync(id, request);
                
                if (success)
                    return Ok(new { message = "Refund initiated successfully" });
                else
                    return BadRequest(new { message = "Failed to process refund" });
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
                _logger.LogError(ex, "Error processing refund for booking {BookingId}", id);
                return StatusCode(500, new { message = "An error occurred while processing the refund" });
            }
        }

        /// <summary>
        /// Webhook endpoint for payment notifications from Xendit
        /// </summary>
        /// <param name="webhookData">Webhook payload</param>
        /// <returns>Processing acknowledgment</returns>
        [HttpPost("webhooks/payment")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentWebhook([FromBody] object webhookData)
        {
            try
            {
                var webhookPayload = webhookData.ToString() ?? "";
                var signature = Request.Headers["x-callback-token"].FirstOrDefault() ?? "";

                var processed = await _bookingService.ProcessPaymentWebhookAsync(webhookPayload, signature);

                if (processed)
                {
                    return Ok(new { message = "Webhook processed successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to process webhook" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment webhook");
                return StatusCode(500, new { message = "An error occurred while processing the webhook" });
            }
        }

        #region Private Helper Methods

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }
            return userId;
        }

        private string GetUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        #endregion
    }
}