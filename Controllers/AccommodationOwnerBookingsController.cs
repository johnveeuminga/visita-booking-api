using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VisitaBookingApi.Data;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/accommodation-owner/my-bookings")]
    public class AccommodationOwnerBookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccommodationOwnerBookingsController> _logger;

        public AccommodationOwnerBookingsController(ApplicationDbContext context, ILogger<AccommodationOwnerBookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get bookings for all accommodations owned by the current user
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="status">Filter by booking status (optional)</param>
        /// <param name="paymentStatus">Filter by payment status (optional)</param>
        /// <param name="checkInDateFrom">Filter by check-in date from (optional)</param>
        /// <param name="checkInDateTo">Filter by check-in date to (optional)</param>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] BookingStatus? status = null,
            [FromQuery] PaymentStatus? paymentStatus = null,
            [FromQuery] DateTime? checkInDateFrom = null,
            [FromQuery] DateTime? checkInDateTo = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { Message = "User authentication required" });
                }

                // Ensure the user has the Hotel role (or is Admin)
                if (!User.IsInRole("Hotel") && !User.IsInRole("Admin"))
                {
                    return Forbid("Only users with the Hotel role can access this resource");
                }

                // Get all active accommodations owned by the user
                var accommodationIds = await _context.Accommodations
                    .Where(a => a.OwnerId == currentUserId.Value && a.IsActive)
                    .Select(a => a.Id)
                    .ToListAsync();

                if (!accommodationIds.Any())
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

                // Get room ids for those accommodations
                var roomIds = await _context.Rooms
                    .Where(r => r.AccommodationId.HasValue && accommodationIds.Contains(r.AccommodationId.Value) && r.IsActive)
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

                // Build bookings query
                var query = _context.Bookings
                    .Include(b => b.Room).ThenInclude(r => r.Accommodation)
                    .Where(b => b.Room != null && roomIds.Contains(b.RoomId))
                    .AsQueryable();

                if (status.HasValue)
                    query = query.Where(b => b.Status == status.Value);

                if (paymentStatus.HasValue)
                    query = query.Where(b => b.PaymentStatus == paymentStatus.Value);

                if (checkInDateFrom.HasValue)
                    query = query.Where(b => b.CheckInDate >= checkInDateFrom.Value);

                if (checkInDateTo.HasValue)
                    query = query.Where(b => b.CheckInDate <= checkInDateTo.Value);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
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
                        ,
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
                            ActiveRoomCount = 0
                        } : null
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = pageNumber < totalPages,
                    HasPreviousPage = pageNumber > 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my bookings");
                return StatusCode(500, new { message = "An error occurred while retrieving bookings" });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) ? null : userId;
        }
    }
}
