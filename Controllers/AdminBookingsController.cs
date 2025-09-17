using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitaBookingApi.Data;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/admin/bookings")]
    [Authorize(Roles = "Admin")]
    public class AdminBookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookingService _bookingService;
        private readonly ILogger<AdminBookingsController> _logger;

        public AdminBookingsController(ApplicationDbContext context, IBookingService bookingService, ILogger<AdminBookingsController> logger)
        {
            _context = context;
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Search bookings (Admin) - filterable by status, accommodationId, check-in/check-out range, guest email/reference
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<BookingSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchBookings(
            [FromQuery] BookingStatus? status = null,
            [FromQuery] PaymentStatus? paymentStatus = null,
            [FromQuery] int? accommodationId = null,
            [FromQuery] DateTime? checkInDateFrom = null,
            [FromQuery] DateTime? checkInDateTo = null,
            [FromQuery] string? bookingReference = null,
            [FromQuery] string? guest = null,
            [FromQuery] string? q = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // Build a single EF query that includes necessary related data and projects to DTOs to avoid extra round trips.
                var query = _context.Bookings
                    .AsNoTracking()
                    .Include(b => b.Room).ThenInclude(r => r.Accommodation)
                    .AsQueryable();

                if (status.HasValue)
                    query = query.Where(b => b.Status == status.Value);

                if (paymentStatus.HasValue)
                    query = query.Where(b => b.PaymentStatus == paymentStatus.Value);

                if (accommodationId.HasValue)
                    query = query.Where(b => b.Room != null && b.Room.AccommodationId.HasValue && b.Room.AccommodationId.Value == accommodationId.Value);


                if (!string.IsNullOrEmpty(bookingReference))
                    query = query.Where(b => b.BookingReference.Contains(bookingReference));

                // 'guest' searches guest name OR guest email (case-insensitive)
                if (!string.IsNullOrEmpty(guest))
                {
                    var loweredGuest = guest.ToLower();
                    query = query.Where(b => b.GuestName.ToLower().Contains(loweredGuest) || b.GuestEmail.ToLower().Contains(loweredGuest));
                }

                // combined 'q' search searches booking reference, room name, guest name or guest email (case-insensitive)
                if (!string.IsNullOrEmpty(q))
                {
                    var loweredQ = q.ToLower();
                    query = query.Where(b => b.BookingReference.ToLower().Contains(loweredQ)
                        || (b.Room != null && b.Room.Name.ToLower().Contains(loweredQ))
                        || b.GuestName.ToLower().Contains(loweredQ)
                        || b.GuestEmail.ToLower().Contains(loweredQ));
                }

                if (checkInDateFrom.HasValue)
                    query = query.Where(b => b.CheckInDate >= checkInDateFrom.Value);

                if (checkInDateTo.HasValue)
                    query = query.Where(b => b.CheckInDate <= checkInDateTo.Value);

                var total = await query.CountAsync();

                var items = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b => new BookingSummaryDto
                    {
                        Id = b.Id,
                        BookingReference = b.BookingReference,
                        RoomName = b.Room != null ? b.Room.Name : string.Empty,
                        CheckInDate = b.CheckInDate,
                        CheckOutDate = b.CheckOutDate,
                        NumberOfNights = b.NumberOfNights,
                        TotalAmount = b.TotalAmount,
                        Status = b.Status,
                        StatusDescription = b.Status.ToString(),
                        PaymentStatus = b.PaymentStatus,
                        PaymentStatusDescription = b.PaymentStatus.ToString(),
                        GuestName = b.GuestName,
                        CreatedAt = b.CreatedAt,
                        Accommodation = b.Room != null && b.Room.Accommodation != null ? new AccommodationSummaryDto
                        {
                            Id = b.Room.Accommodation.Id,
                            Name = b.Room.Accommodation.Name,
                            Description = b.Room.Accommodation.Description,
                            Logo = b.Room.Accommodation.Logo,
                            Address = b.Room.Accommodation.Address,
                            EmailAddress = b.Room.Accommodation.EmailAddress,
                            ContactNo = b.Room.Accommodation.ContactNo,
                            IsActive = b.Room.Accommodation.IsActive,
                            Status = b.Room.Accommodation.Status.ToString(),
                            ActiveRoomCount = 0,
                            PhotoUrls = b.Room.Accommodation.Logo != null ? new List<string> { b.Room.Accommodation.Logo } : new List<string>()
                        } : null
                    })
                    .ToListAsync();

                return Ok(new PagedResult<BookingSummaryDto>
                {
                    Items = items,
                    TotalCount = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching bookings (admin)");
                return StatusCode(500, new { message = "An error occurred while searching bookings" });
            }
        }

        /// <summary>
        /// Get booking details by id (Admin)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingDetails(int id)
        {
            try
            {
                // Single efficient query projecting to BookingResponseDto including payments, reservation, room and accommodation summaries.
                var booking = await _context.Bookings
                    .AsNoTracking()
                    .Where(b => b.Id == id)
                    .Select(b => new BookingResponseDto
                    {
                        Id = b.Id,
                        BookingReference = b.BookingReference,
                        RoomId = b.RoomId,
                        RoomName = b.Room != null ? b.Room.Name : string.Empty,
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
                        CancelledAt = b.CancelledAt,
                        CancellationReason = b.CancellationReason,
                        ActualCheckInAt = b.ActualCheckInAt,
                        ActualCheckOutAt = b.ActualCheckOutAt,

                        Reservation = b.Reservation != null ? new BookingReservationDto
                        {
                            Id = b.Reservation.Id,
                            ReservationReference = b.Reservation.ReservationReference,
                            Status = b.Reservation.Status,
                            ReservedAt = b.Reservation.ReservedAt,
                            ExpiresAt = b.Reservation.ExpiresAt,
                            PaymentUrl = b.Reservation.PaymentUrl,
                            PaymentUrlExpiresAt = b.Reservation.PaymentUrlExpiresAt,
                            ExtensionCount = b.Reservation.ExtensionCount,
                            CanExtend = b.Reservation.ExtensionCount < 2,
                            IsActive = b.Reservation.Status == ReservationStatus.Active,
                            IsExpired = b.Reservation.Status == ReservationStatus.Expired,
                            // compute TimeUntilExpiry after materialization to avoid EF type coercion issues
                            TimeUntilExpiry = null
                        } : null,

                        Payments = b.Payments.Select(p => new BookingPaymentDto
                        {
                            Id = p.Id,
                            PaymentReference = p.PaymentReference,
                            PaymentType = p.PaymentType,
                            PaymentMethod = p.PaymentMethod,
                            Status = p.Status,
                            Amount = p.Amount,
                            Currency = p.Currency,
                            NetAmount = p.NetAmount,
                            CreatedAt = p.CreatedAt,
                            ProcessedAt = p.ProcessedAt,
                            ConfirmedAt = p.ConfirmedAt,
                            FailureReason = p.FailureReason,
                            CardLastFour = p.CardLastFour,
                            BankCode = p.BankCode,
                            PaymentDescription = p.PaymentDescription
                        }).ToList(),

                        Room = b.Room != null ? new RoomSummaryDto
                        {
                            Id = b.Room.Id,
                            Name = b.Room.Name,
                            Description = b.Room.Description,
                            DefaultPrice = b.Room.DefaultPrice,
                            MaxGuests = b.Room.MaxGuests,
                            MainPhotoUrl = b.Room.MainPhotoUrl,
                            Photos = b.Room.Photos.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).Select(p => new RoomPhotoDTO
                            {
                                Id = p.Id,
                                FileName = p.FileName,
                                FileUrl = p.CdnUrl ?? p.S3Url,
                                LastModified = p.LastModified
                            }).ToList()
                        } : null,

                        Accommodation = b.Room != null && b.Room.Accommodation != null ? new AccommodationSummaryDto
                        {
                            Id = b.Room.Accommodation.Id,
                            Name = b.Room.Accommodation.Name,
                            Description = b.Room.Accommodation.Description,
                            Logo = b.Room.Accommodation.Logo,
                            Address = b.Room.Accommodation.Address,
                            EmailAddress = b.Room.Accommodation.EmailAddress,
                            ContactNo = b.Room.Accommodation.ContactNo,
                            IsActive = b.Room.Accommodation.IsActive,
                            Status = b.Room.Accommodation.Status.ToString()
                        ,
                            PhotoUrls = b.Room.Accommodation.Logo != null ? new List<string> { b.Room.Accommodation.Logo } : new List<string>()
                        } : null,

                        AdditionalFees = b.Payments
                            .Where(p => p.PaymentType == PaymentType.Fee || p.PaymentType == PaymentType.Adjustment)
                            .Select(p => new BookingFeeDto
                            {
                                FeeType = p.PaymentType.ToString(),
                                Description = p.PaymentDescription,
                                Amount = p.Amount,
                                IsPercentage = false,
                                PercentageRate = null
                            }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (booking == null)
                    return NotFound(new { message = "Booking not found" });

                // compute TimeUntilExpiry on the DTO after EF materialization to avoid server-side expression translation between DateTime and TimeSpan
                if (booking.Reservation != null && booking.Reservation.ExpiresAt != default)
                {
                    var timeUntil = booking.Reservation.ExpiresAt - DateTime.UtcNow;
                    booking.Reservation.TimeUntilExpiry = timeUntil;
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking details for {BookingId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving booking details" });
            }
        }

        /// <summary>
        /// Cancel a booking (Admin)
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request" });

                var adminUserId = GetCurrentUserId() ?? -1;
                var cancelled = await _bookingService.CancelBookingAsync(id, request, adminUserId);
                return Ok(cancelled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
                return StatusCode(500, new { message = "An error occurred while cancelling the booking" });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) ? null : userId;
        }
    }
}
