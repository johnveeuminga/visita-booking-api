using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VisitaBookingApi.Data;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/accommodations/{accommodationId}/bookings")]
    public class AccommodationBookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccommodationBookingsController> _logger;

        public AccommodationBookingsController(
            IBookingService bookingService,
            ApplicationDbContext context,
            ILogger<AccommodationBookingsController> logger)
        {
            _bookingService = bookingService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all bookings for a specific accommodation
        /// </summary>
        /// <param name="accommodationId">Accommodation ID</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="status">Filter by booking status (optional)</param>
        /// <param name="paymentStatus">Filter by payment status (optional)</param>
        /// <param name="checkInDateFrom">Filter by check-in date from (optional)</param>
        /// <param name="checkInDateTo">Filter by check-in date to (optional)</param>
        /// <returns>Paginated list of bookings for the accommodation</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAccommodationBookings(
            int accommodationId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] BookingStatus? status = null,
            [FromQuery] PaymentStatus? paymentStatus = null,
            [FromQuery] DateTime? checkInDateFrom = null,
            [FromQuery] DateTime? checkInDateTo = null)
        {
            try
            {
                // Verify accommodation exists and user has access
                var accessCheck = await ValidateAccommodationAccessAsync(accommodationId);
                if (accessCheck != null) return accessCheck;

                // Get room IDs for this accommodation
                var roomIds = await _context.Rooms
                    .Where(r => r.AccommodationId == accommodationId && r.IsActive)
                    .Select(r => r.Id)
                    .ToListAsync();

                if (!roomIds.Any())
                {
                    return Ok(new
                    {
                        Items = new List<object>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalPages = 0,
                        HasNextPage = false,
                        HasPreviousPage = false
                    });
                }

                // Create search request for bookings in accommodation's rooms
                var searchRequest = new AccommodationBookingSearchDto
                {
                    RoomIds = roomIds,
                    Status = status,
                    PaymentStatus = paymentStatus,
                    CheckInDateFrom = checkInDateFrom,
                    CheckInDateTo = checkInDateTo,
                    PageNumber = pageNumber,
                    PageSize = Math.Min(pageSize, 100) // Cap at 100 items per page
                };

                var results = await SearchAccommodationBookingsAsync(searchRequest);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for accommodation {AccommodationId}", accommodationId);
                return StatusCode(500, new { message = "An error occurred while retrieving bookings" });
            }
        }

        /// <summary>
        /// Get booking statistics for a specific accommodation
        /// </summary>
        /// <param name="accommodationId">Accommodation ID</param>
        /// <param name="dateFrom">Statistics date range start (optional)</param>
        /// <param name="dateTo">Statistics date range end (optional)</param>
        /// <returns>Booking statistics and metrics for the accommodation</returns>
        [HttpGet("statistics")]
        [Authorize]
        public async Task<IActionResult> GetAccommodationBookingStatistics(
            int accommodationId,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            try
            {
                // Verify accommodation exists and user has access
                var accessCheck = await ValidateAccommodationAccessAsync(accommodationId);
                if (accessCheck != null) return accessCheck;

                var stats = await GetAccommodationStatisticsAsync(accommodationId, dateFrom, dateTo);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking statistics for accommodation {AccommodationId}", accommodationId);
                return StatusCode(500, new { message = "An error occurred while retrieving booking statistics" });
            }
        }

        /// <summary>
        /// Get upcoming check-ins for a specific accommodation
        /// </summary>
        /// <param name="accommodationId">Accommodation ID</param>
        /// <param name="days">Number of days to look ahead (default: 7)</param>
        /// <returns>List of upcoming check-ins</returns>
        [HttpGet("upcoming-checkins")]
        [Authorize]
        public async Task<IActionResult> GetUpcomingCheckIns(
            int accommodationId,
            [FromQuery] int days = 7)
        {
            try
            {
                // Verify accommodation exists and user has access
                var accessCheck = await ValidateAccommodationAccessAsync(accommodationId);
                if (accessCheck != null) return accessCheck;

                var fromDate = DateTime.Today;
                var toDate = DateTime.Today.AddDays(days);

                var upcomingCheckIns = await _context.Bookings
                    .Include(b => b.Room).ThenInclude(r => r.Accommodation)
                    .Where(b => b.Room != null && 
                               b.Room.AccommodationId == accommodationId &&
                               b.CheckInDate >= fromDate && 
                               b.CheckInDate <= toDate &&
                               (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Reserved) &&
                               b.PaymentStatus == PaymentStatus.Paid)
                    .OrderBy(b => b.CheckInDate)
                    .ThenBy(b => b.CreatedAt)
                    .Select(b => new AccommodationCheckInDto
                    {
                        BookingId = b.Id,
                        BookingReference = b.BookingReference,
                        GuestName = b.GuestName,
                        GuestEmail = b.GuestEmail,
                        GuestPhone = b.GuestPhone,
                        CheckInDate = b.CheckInDate,
                        CheckOutDate = b.CheckOutDate,
                        NumberOfGuests = b.NumberOfGuests,
                        NumberOfNights = b.NumberOfNights,
                        RoomId = b.RoomId,
                        RoomName = b.Room != null ? b.Room.Name : "Unknown Room",
                        Status = b.Status,
                        SpecialRequests = b.SpecialRequests
                    })
                    .ToListAsync();

                return Ok(upcomingCheckIns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming check-ins for accommodation {AccommodationId}", accommodationId);
                return StatusCode(500, new { message = "An error occurred while retrieving upcoming check-ins" });
            }
        }

        /// <summary>
        /// Get current guests (checked-in) for a specific accommodation
        /// </summary>
        /// <param name="accommodationId">Accommodation ID</param>
        /// <returns>List of current guests</returns>
        [HttpGet("current-guests")]
        [Authorize]
        public async Task<IActionResult> GetCurrentGuests(int accommodationId)
        {
            try
            {
                // Verify accommodation exists and user has access
                var accessCheck = await ValidateAccommodationAccessAsync(accommodationId);
                if (accessCheck != null) return accessCheck;

                var currentGuests = await _context.Bookings
                    .Include(b => b.Room).ThenInclude(r => r.Accommodation)
                    .Where(b => b.Room != null && 
                               b.Room.AccommodationId == accommodationId &&
                               b.Status == BookingStatus.CheckedIn)
                    .OrderBy(b => b.CheckOutDate)
                    .Select(b => new AccommodationCheckInDto
                    {
                        BookingId = b.Id,
                        BookingReference = b.BookingReference,
                        GuestName = b.GuestName,
                        GuestEmail = b.GuestEmail,
                        GuestPhone = b.GuestPhone,
                        CheckInDate = b.CheckInDate,
                        CheckOutDate = b.CheckOutDate,
                        NumberOfGuests = b.NumberOfGuests,
                        NumberOfNights = b.NumberOfNights,
                        RoomId = b.RoomId,
                        RoomName = b.Room != null ? b.Room.Name : "Unknown Room",
                        Status = b.Status,
                        SpecialRequests = b.SpecialRequests,
                        ActualCheckInAt = b.ActualCheckInAt
                    })
                    .ToListAsync();

                return Ok(currentGuests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current guests for accommodation {AccommodationId}", accommodationId);
                return StatusCode(500, new { message = "An error occurred while retrieving current guests" });
            }
        }

        /// <summary>
        /// Update booking status (Accommodation owner/manager only)
        /// </summary>
        /// <param name="accommodationId">Accommodation ID</param>
        /// <param name="bookingId">Booking ID</param>
        /// <param name="request">Status update request</param>
        /// <returns>Updated booking details</returns>
        [HttpPut("{bookingId}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateBookingStatus(
            int accommodationId, 
            int bookingId, 
            [FromBody] UpdateBookingStatusDto request)
        {
            try
            {
                // Verify accommodation exists and user has access
                var accessCheck = await ValidateAccommodationAccessAsync(accommodationId);
                if (accessCheck != null) return accessCheck;

                // Verify booking belongs to this accommodation
                var booking = await _context.Bookings
                    .Include(b => b.Room).ThenInclude(r => r.Accommodation)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && 
                                            b.Room != null && 
                                            b.Room.AccommodationId == accommodationId);

                if (booking == null)
                {
                    return NotFound(new { message = "Booking not found in this accommodation" });
                }

                request.UpdatedBy = GetUserEmail();
                var updatedBooking = await _bookingService.UpdateBookingStatusAsync(bookingId, request);
                return Ok(updatedBooking);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status for accommodation {AccommodationId}, booking {BookingId}", 
                    accommodationId, bookingId);
                return StatusCode(500, new { message = "An error occurred while updating the booking status" });
            }
        }

        #region Private Helper Methods

        private async Task<IActionResult?> ValidateAccommodationAccessAsync(int accommodationId)
        {
            // Check if accommodation exists and is active
            var accommodation = await _context.Accommodations
                .Where(a => a.Id == accommodationId && a.IsActive)
                .FirstOrDefaultAsync();

            if (accommodation == null)
            {
                return NotFound(new { Message = "Accommodation not found or inactive" });
            }

            // Check if user can access this accommodation
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { Message = "User authentication required" });
            }

            // Allow admins or accommodation owners
            if (!IsAdmin() && accommodation.OwnerId != currentUserId.Value)
            {
                return Forbid("You can only access bookings for accommodations you own");
            }

            return null; // Validation passed
        }

        private async Task<object> SearchAccommodationBookingsAsync(AccommodationBookingSearchDto searchRequest)
        {
            var query = _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.Accommodation)
                .Where(b => b.Room != null && searchRequest.RoomIds.Contains(b.RoomId))
                .AsQueryable();

            // Apply filters
            if (searchRequest.Status.HasValue)
                query = query.Where(b => b.Status == searchRequest.Status.Value);

            if (searchRequest.PaymentStatus.HasValue)
                query = query.Where(b => b.PaymentStatus == searchRequest.PaymentStatus.Value);

            if (searchRequest.CheckInDateFrom.HasValue)
                query = query.Where(b => b.CheckInDate >= searchRequest.CheckInDateFrom.Value);

            if (searchRequest.CheckInDateTo.HasValue)
                query = query.Where(b => b.CheckInDate <= searchRequest.CheckInDateTo.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize)
                .Select(b => new AccommodationBookingDto
                {
                    Id = b.Id,
                    BookingReference = b.BookingReference,
                    RoomId = b.RoomId,
                    RoomName = b.Room != null ? b.Room.Name : "Unknown Room",
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    NumberOfGuests = b.NumberOfGuests,
                    NumberOfNights = b.NumberOfNights,
                    BaseAmount = b.BaseAmount,
                    TaxAmount = b.TaxAmount,
                    ServiceFee = b.ServiceFee,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
                    GuestName = b.GuestName,
                    GuestEmail = b.GuestEmail,
                    GuestPhone = b.GuestPhone,
                    SpecialRequests = b.SpecialRequests,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    ActualCheckInAt = b.ActualCheckInAt,
                    ActualCheckOutAt = b.ActualCheckOutAt
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize);

            return new
            {
                Items = bookings,
                TotalCount = totalCount,
                PageNumber = searchRequest.PageNumber,
                PageSize = searchRequest.PageSize,
                TotalPages = totalPages,
                HasNextPage = searchRequest.PageNumber < totalPages,
                HasPreviousPage = searchRequest.PageNumber > 1
            };
        }

        private async Task<object> GetAccommodationStatisticsAsync(int accommodationId, DateTime? dateFrom, DateTime? dateTo)
        {
            var fromDate = dateFrom ?? DateTime.Today.AddMonths(-1);
            var toDate = dateTo ?? DateTime.Today;

            var roomIds = await _context.Rooms
                .Where(r => r.AccommodationId == accommodationId && r.IsActive)
                .Select(r => r.Id)
                .ToListAsync();

            var bookingsQuery = _context.Bookings
                .Where(b => roomIds.Contains(b.RoomId) && 
                           b.CreatedAt >= fromDate && 
                           b.CreatedAt <= toDate.AddDays(1));

            var totalBookings = await bookingsQuery.CountAsync();
            var confirmedBookings = await bookingsQuery.CountAsync(b => b.Status == BookingStatus.Confirmed);
            var cancelledBookings = await bookingsQuery.CountAsync(b => b.Status == BookingStatus.Cancelled);
            var totalRevenue = await bookingsQuery
                .Where(b => b.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(b => b.TotalAmount);

            var occupancyRate = roomIds.Any() ? 
                (double)confirmedBookings / (roomIds.Count * (toDate - fromDate).Days) * 100 : 0;

            return new
            {
                Period = new { From = fromDate, To = toDate },
                TotalBookings = totalBookings,
                ConfirmedBookings = confirmedBookings,
                CancelledBookings = cancelledBookings,
                CancellationRate = totalBookings > 0 ? (double)cancelledBookings / totalBookings * 100 : 0,
                TotalRevenue = totalRevenue,
                AverageBookingValue = confirmedBookings > 0 ? totalRevenue / confirmedBookings : 0,
                OccupancyRate = Math.Round(occupancyRate, 2),
                TotalRooms = roomIds.Count
            };
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) ? null : userId;
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