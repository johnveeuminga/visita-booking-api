using System.Text.Json;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;
using VisitaBookingApi.Services.Interfaces;

namespace visita_booking_api.Services.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IXenditService _xenditService;
        private readonly IRoomCalendarService _roomCalendarService;
        private readonly IAvailabilityLedgerService _ledgerService;
        private readonly BookingConfiguration _config;
        private readonly ILogger<BookingService> _logger;
        private readonly IDistributedLockService _distributedLock;
        private readonly ITimezoneService _timezoneService;
        private readonly visita_booking_api.Services.Interfaces.IEmailService _emailService;

        public BookingService(
            ApplicationDbContext context,
            IXenditService xenditService,
            IRoomCalendarService roomCalendarService,
            IAvailabilityLedgerService ledgerService,
            IOptions<BookingConfiguration> config,
            ILogger<BookingService> logger,
            IDistributedLockService distributedLock,
            ITimezoneService timezoneService,
            visita_booking_api.Services.Interfaces.IEmailService emailService
        )
        {
            _context = context;
            _xenditService = xenditService;
            _roomCalendarService = roomCalendarService;
            _ledgerService = ledgerService;
            _config = config.Value;
            _logger = logger;
            _distributedLock = distributedLock;
            _timezoneService = timezoneService;
            _emailService = emailService;
        }

        public async Task<BookingAvailabilityResponseDto> CheckAvailabilityAsync(
            BookingAvailabilityRequestDto request
        )
        {
            try
            {
                // Validate dates
                if (request.CheckInDate < _timezoneService.Today)
                {
                    return new BookingAvailabilityResponseDto
                    {
                        IsAvailable = false,
                        Message = "Check-in date cannot be in the past",
                    };
                }

                if (request.CheckOutDate <= request.CheckInDate)
                {
                    return new BookingAvailabilityResponseDto
                    {
                        IsAvailable = false,
                        Message = "Check-out date must be after check-in date",
                    };
                }

                // Get room details
                var room = await _context
                    .Rooms.Include(r => r.Accommodation)
                    .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.IsActive);

                if (room == null)
                {
                    return new BookingAvailabilityResponseDto
                    {
                        IsAvailable = false,
                        Message = "Room not found or not available",
                    };
                }

                // Check guest capacity
                if (request.NumberOfGuests > room.MaxGuests)
                {
                    return new BookingAvailabilityResponseDto
                    {
                        IsAvailable = false,
                        Message = $"Room capacity exceeded. Maximum guests: {room.MaxGuests}",
                    };
                }

                // Check room availability
                var isAvailable = await IsRoomAvailableAsync(
                    request.RoomId,
                    request.CheckInDate,
                    request.CheckOutDate
                );
                if (!isAvailable.IsAvailable)
                {
                    return new BookingAvailabilityResponseDto
                    {
                        IsAvailable = false,
                        Message = isAvailable.Message,
                        UnavailableDates = isAvailable.ConflictingDates,
                    };
                }

                // Calculate pricing
                var pricing = await CalculatePricingAsync(
                    request.RoomId,
                    request.CheckInDate,
                    request.CheckOutDate,
                    request.NumberOfGuests
                );

                return new BookingAvailabilityResponseDto
                {
                    IsAvailable = true,
                    Message = "Room is available for the selected dates",
                    EstimatedPrice = pricing.TotalAmount,
                    PricingDetails = pricing,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking availability for room {RoomId} from {CheckIn} to {CheckOut}",
                    request.RoomId,
                    request.CheckInDate,
                    request.CheckOutDate
                );

                return new BookingAvailabilityResponseDto
                {
                    IsAvailable = false,
                    Message = "Error checking availability. Please try again.",
                };
            }
        }

        public async Task<BookingResponseDto> CreateBookingAsync(
            CreateBookingRequestDto request,
            int userId
        )
        {
            // Multi-Date Distributed Locking - prevents concurrent booking of overlapping date ranges
            var lockToken = await _distributedLock.AcquireBookingLockAsync(
                request.RoomId,
                request.CheckInDate,
                request.CheckOutDate,
                TimeSpan.FromMinutes(2)
            );

            if (lockToken == null)
            {
                throw new InvalidOperationException(
                    "Another user is currently booking this room for overlapping dates. Please try again in a few moments."
                );
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate availability with the distributed locks held for all dates
                var availability = await CheckAvailabilityAsync(
                    new BookingAvailabilityRequestDto
                    {
                        RoomId = request.RoomId,
                        CheckInDate = request.CheckInDate,
                        CheckOutDate = request.CheckOutDate,
                        NumberOfGuests = request.NumberOfGuests,
                    }
                );

                if (!availability.IsAvailable)
                {
                    throw new InvalidOperationException(availability.Message);
                }

                // Generate booking reference
                var bookingReference = await GenerateBookingReferenceAsync();
                var numberOfNights = (request.CheckOutDate - request.CheckInDate).Days;

                // Calculate final pricing
                var pricing = availability.PricingDetails!;

                // Normalize incoming dates to UTC (assume dates are provided as local dates or unspecified)
                var checkInUtc = request.CheckInDate;
                var checkOutUtc = request.CheckOutDate;
                if (checkInUtc.Kind == DateTimeKind.Unspecified)
                {
                    // Treat date-only values as UTC midnight
                    checkInUtc = DateTime.SpecifyKind(checkInUtc.Date, DateTimeKind.Utc);
                }
                else if (checkInUtc.Kind == DateTimeKind.Local)
                {
                    checkInUtc = checkInUtc.ToUniversalTime();
                }

                if (checkOutUtc.Kind == DateTimeKind.Unspecified)
                {
                    checkOutUtc = DateTime.SpecifyKind(checkOutUtc.Date, DateTimeKind.Utc);
                }
                else if (checkOutUtc.Kind == DateTimeKind.Local)
                {
                    checkOutUtc = checkOutUtc.ToUniversalTime();
                }

                // Create booking
                var booking = new Booking
                {
                    BookingReference = bookingReference,
                    UserId = userId,
                    RoomId = request.RoomId,
                    CheckInDate = checkInUtc,
                    CheckOutDate = checkOutUtc,
                    NumberOfGuests = request.NumberOfGuests,
                    NumberOfNights = numberOfNights,
                    BaseAmount = pricing.BaseAmount,
                    TaxAmount = pricing.TaxAmount,
                    ServiceFee = pricing.ServiceFee,
                    TotalAmount = pricing.TotalAmount,
                    Status = BookingStatus.Reserved,
                    PaymentStatus = PaymentStatus.Pending,
                    GuestName = request.GuestName,
                    GuestEmail = request.GuestEmail,
                    GuestPhone = request.GuestPhone,
                    SpecialRequests = request.SpecialRequests,
                    CreatedBy = userId.ToString(),
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Create reservation with GMT+8 timezone
                var reservationReference = $"RES-{bookingReference}";
                // Compute expiry in UTC based on local timezone now

                var expiryUtc = DateTime.UtcNow.AddMinutes(
                    request.ReservationTimeoutMinutes ?? _config.DefaultReservationTimeoutMinutes
                );
                var reservation = new BookingReservation
                {
                    ReservationReference = reservationReference,
                    BookingId = booking.Id,
                    UserId = userId,
                    RoomId = request.RoomId,
                    CheckInDate = checkInUtc,
                    CheckOutDate = checkOutUtc,
                    NumberOfGuests = request.NumberOfGuests,
                    TotalAmount = pricing.TotalAmount,
                    Status = ReservationStatus.Active,
                    ExpiresAt = expiryUtc,
                };

                _context.BookingReservations.Add(reservation);
                await _context.SaveChangesAsync();

                // Create Xendit invoice with exact same expiry as reservation
                var invoiceResult = await _xenditService.CreateInvoiceAsync(
                    booking,
                    reservation.ExpiresAt
                );

                Console.WriteLine("Invoice REsult: " + invoiceResult.PaymentUrl);

                if (invoiceResult.IsSuccess)
                {
                    reservation.XenditInvoiceId = invoiceResult.InvoiceId;
                    reservation.PaymentUrl = invoiceResult.PaymentUrl;
                    reservation.PaymentUrlExpiresAt = invoiceResult.ExpiryDate;
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to create Xendit invoice for booking {BookingId}: {Error}",
                        booking.Id,
                        invoiceResult.ErrorMessage
                    );
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Created booking {BookingReference} for user {UserId}",
                    bookingReference,
                    userId
                );

                return await MapToBookingResponseAsync(booking);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating booking for user {UserId}", userId);
                throw;
            }
            finally
            {
                // Always release all distributed locks for the booked dates
                if (lockToken != null)
                {
                    await _distributedLock.ReleaseBookingLockAsync(
                        request.RoomId,
                        request.CheckInDate,
                        request.CheckOutDate,
                        lockToken
                    );
                }
            }
        }

        public async Task<BookingResponseDto> ConfirmBookingAsync(
            int bookingId,
            string paymentReference
        )
        {
            // If a transaction already exists on the DbContext (caller started one), join it.
            // Otherwise, create a new transaction for this operation.
            var currentTransaction = _context.Database.CurrentTransaction;
            var createdTransaction =
                currentTransaction == null ? await _context.Database.BeginTransactionAsync() : null;
            try
            {
                var booking = await _context
                    .Bookings.Include(b => b.Reservation)
                    .Include(b => b.Payments)
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Accommodation)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                    throw new ArgumentException("Booking not found");

                if (booking.Status != BookingStatus.Reserved)
                    throw new InvalidOperationException(
                        $"Cannot confirm booking with status: {booking.Status}"
                    );

                // Update booking status
                booking.Status = BookingStatus.Confirmed;
                booking.PaymentStatus = PaymentStatus.Paid;
                booking.UpdatedAt = GetCurrentUtcTime();

                // Update reservation status
                if (booking.Reservation != null)
                {
                    booking.Reservation.Status = ReservationStatus.Confirmed;
                    booking.Reservation.CompletedAt = DateTime.UtcNow;
                    booking.Reservation.UpdatedAt = DateTime.UtcNow;
                }

                // Release availability lock
                await ReleaseAvailabilityLockAsync(booking.Id, "Booking confirmed");

                await _context.SaveChangesAsync();

                if (createdTransaction != null)
                {
                    await createdTransaction.CommitAsync();
                }

                _logger.LogInformation(
                    "Confirmed booking {BookingReference}",
                    booking.BookingReference
                );

                // Warmup ledger for the affected room for the next 6 months so ledger-backed searches pick up the change.
                // Only perform warmup if this method created the transaction (caller is not managing transactions)
                try
                {
                    if (createdTransaction != null && booking.RoomId > 0)
                    {
                        var start = booking.CheckInDate.Date;
                        var end = start.AddMonths(6).Date; // end exclusive
                        await _ledgerService.WarmupRoomLedgerAsync(booking.RoomId, start, end);
                        // Also invalidate availability cache pattern so consumers refresh immediately
                        // Note: BookingService doesn't have ICacheInvalidationService, we rely on callers (e.g., SQS handler) to invalidate.
                    }
                }
                catch (Exception lex)
                {
                    _logger.LogWarning(
                        lex,
                        "Failed to warmup ledger after confirming booking {BookingRef}",
                        booking.BookingReference
                    );
                }

                // Send booking confirmation email (best-effort)
                try
                {
                    await _emailService.SendBookingConfirmationAsync(
                        booking,
                        booking.BaseAmount,
                        booking.ServiceFee
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send booking confirmation email for booking {BookingId}",
                        booking.Id
                    );
                }

                return await MapToBookingResponseAsync(booking);
            }
            catch (Exception ex)
            {
                if (createdTransaction != null)
                {
                    await createdTransaction.RollbackAsync();
                }

                _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
                throw;
            }
            finally
            {
                if (createdTransaction != null)
                {
                    await createdTransaction.DisposeAsync();
                }
            }
        }

        public async Task<BookingResponseDto> CancelBookingAsync(
            int bookingId,
            CancelBookingRequestDto request,
            int userId
        )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context
                    .Bookings.Include(b => b.Reservation)
                    .Include(b => b.Payments)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                    throw new ArgumentException("Booking not found");

                if (booking.UserId != userId)
                    throw new UnauthorizedAccessException("You can only cancel your own bookings");

                if (booking.Status == BookingStatus.Cancelled)
                    throw new InvalidOperationException("Booking is already cancelled");

                if (booking.Status == BookingStatus.CheckedOut)
                    throw new InvalidOperationException("Cannot cancel a completed booking");

                // Update booking status
                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancelledBy = userId.ToString();
                booking.CancellationReason = request.CancellationReason;
                booking.UpdatedAt = DateTime.UtcNow;

                // Update reservation status
                if (booking.Reservation != null)
                {
                    booking.Reservation.Status = ReservationStatus.Cancelled;
                    booking.Reservation.CancelledAt = DateTime.UtcNow;
                    booking.Reservation.CancellationReason = request.CancellationReason;
                    booking.Reservation.UpdatedAt = DateTime.UtcNow;

                    // Expire Xendit invoice if exists
                    if (!string.IsNullOrEmpty(booking.Reservation.XenditInvoiceId))
                    {
                        await _xenditService.ExpireInvoiceAsync(
                            booking.Reservation.XenditInvoiceId
                        );
                    }
                }

                // Release availability lock
                await ReleaseAvailabilityLockAsync(booking.Id, "Booking cancelled");

                // Process refund if applicable
                if (request.RequestRefund && booking.PaymentStatus == PaymentStatus.Paid)
                {
                    var successfulPayment = booking.Payments.FirstOrDefault(p =>
                        p.Status == PaymentStatus.Paid
                    );
                    if (successfulPayment != null)
                    {
                        await ProcessRefundAsync(
                            bookingId,
                            new RefundPaymentRequestDto
                            {
                                BookingReference = booking.BookingReference,
                                RefundReason = request.CancellationReason,
                                ProcessedBy = request.CancelledBy,
                            }
                        );
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Cancelled booking {BookingReference} by user {UserId}",
                    booking.BookingReference,
                    userId
                );

                return await MapToBookingResponseAsync(booking);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(
                    ex,
                    "Error cancelling booking {BookingId} by user {UserId}",
                    bookingId,
                    userId
                );
                throw;
            }
        }

        public async Task<BookingReservationDto> ExtendReservationAsync(
            ExtendReservationRequestDto request,
            int userId
        )
        {
            try
            {
                var reservation = await _context
                    .BookingReservations.Include(r => r.Booking)
                    .FirstOrDefaultAsync(r =>
                        r.ReservationReference == request.ReservationReference && r.UserId == userId
                    );

                if (reservation == null)
                    throw new ArgumentException("Reservation not found");

                if (!reservation.CanExtend)
                    throw new InvalidOperationException("Reservation cannot be extended");

                if (reservation.IsExpired)
                    throw new InvalidOperationException("Reservation has already expired");

                // Extend the reservation
                reservation.ExpiresAt = reservation.ExpiresAt.AddMinutes(request.ExtensionMinutes);
                reservation.ExtensionCount++;
                reservation.LastExtendedAt = DateTime.UtcNow;
                reservation.UpdatedAt = DateTime.UtcNow;

                // Update availability lock if exists
                var availabilityLock = await _context.BookingAvailabilityLocks.FirstOrDefaultAsync(
                    l => l.ReservationId == reservation.Id && l.IsActive
                );

                if (availabilityLock != null)
                {
                    availabilityLock.ExpiresAt = reservation.ExpiresAt;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Extended reservation {ReservationReference} by {Minutes} minutes",
                    request.ReservationReference,
                    request.ExtensionMinutes
                );

                return MapToReservationDto(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error extending reservation {ReservationReference}",
                    request.ReservationReference
                );
                throw;
            }
        }

        public async Task<BookingResponseDto?> GetBookingAsync(int bookingId, int? userId = null)
        {
            var booking = await _context
                .Bookings.Include(b => b.Room)
                .ThenInclude(r => r.Photos)
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomAmenities)
                .ThenInclude(ra => ra.Amenity)
                .Include(b => b.Room)
                .ThenInclude(r => r.Accommodation)
                .Include(b => b.User)
                .Include(b => b.Reservation)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return null;

            // Authorization check for non-admin users
            if (userId.HasValue && booking.UserId != userId.Value)
                return null;

            return await MapToBookingResponseAsync(booking);
        }

        public async Task<BookingResponseDto?> GetBookingByReferenceAsync(
            string bookingReference,
            int? userId = null
        )
        {
            var booking = await _context
                .Bookings.Include(b => b.Room)
                .ThenInclude(r => r.Photos)
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomAmenities)
                .ThenInclude(ra => ra.Amenity)
                .Include(b => b.Room)
                .ThenInclude(r => r.Accommodation)
                .Include(b => b.User)
                .Include(b => b.Reservation)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.BookingReference == bookingReference);

            if (booking == null)
                return null;

            // Authorization check for non-admin users
            if (userId.HasValue && booking.UserId != userId.Value)
                return null;

            return await MapToBookingResponseAsync(booking);
        }

        public async Task<PagedResult<BookingSummaryDto>> SearchBookingsAsync(
            BookingSearchRequestDto request,
            int? userId = null
        )
        {
            var query = _context
                .Bookings.Include(b => b.Room)
                .ThenInclude(r => r.Accommodation)
                .AsQueryable();

            // Apply user filter for non-admin users
            if (userId.HasValue)
                query = query.Where(b => b.UserId == userId.Value);

            // Apply filters
            if (!string.IsNullOrEmpty(request.BookingReference))
                query = query.Where(b => b.BookingReference.Contains(request.BookingReference));

            if (!string.IsNullOrEmpty(request.GuestEmail))
                query = query.Where(b => b.GuestEmail.Contains(request.GuestEmail));

            if (request.Status.HasValue)
                query = query.Where(b => b.Status == request.Status.Value);

            if (request.PaymentStatus.HasValue)
                query = query.Where(b => b.PaymentStatus == request.PaymentStatus.Value);

            if (request.CheckInDateFrom.HasValue)
                query = query.Where(b => b.CheckInDate >= request.CheckInDateFrom.Value);

            if (request.CheckInDateTo.HasValue)
                query = query.Where(b => b.CheckInDate <= request.CheckInDateTo.Value);

            if (request.RoomId.HasValue)
                query = query.Where(b => b.RoomId == request.RoomId.Value);

            if (request.UserId.HasValue && !userId.HasValue) // Admin can search by user ID
                query = query.Where(b => b.UserId == request.UserId.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = request.SortBy?.ToLowerInvariant() switch
            {
                "checkindate" => request.SortDirection?.ToUpperInvariant() == "DESC"
                    ? query.OrderByDescending(b => b.CheckInDate)
                    : query.OrderBy(b => b.CheckInDate),
                "totalamount" => request.SortDirection?.ToUpperInvariant() == "DESC"
                    ? query.OrderByDescending(b => b.TotalAmount)
                    : query.OrderBy(b => b.TotalAmount),
                "status" => request.SortDirection?.ToUpperInvariant() == "DESC"
                    ? query.OrderByDescending(b => b.Status)
                    : query.OrderBy(b => b.Status),
                _ => request.SortDirection?.ToUpperInvariant() == "DESC"
                    ? query.OrderByDescending(b => b.CreatedAt)
                    : query.OrderBy(b => b.CreatedAt),
            };

            // Apply pagination and execute query
            var bookings = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Map to DTOs with status descriptions and accommodation data
            var bookingDtos = bookings
                .Select(b => new BookingSummaryDto
                {
                    Id = b.Id,
                    BookingReference = b.BookingReference,
                    RoomName = b.Room?.Name ?? "",
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    NumberOfNights = b.NumberOfNights,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status,
                    StatusDescription = GetBookingStatusDescription(b.Status),
                    PaymentStatus = b.PaymentStatus,
                    PaymentStatusDescription = GetPaymentStatusDescription(b.PaymentStatus),
                    GuestName = b.GuestName,
                    CreatedAt = b.CreatedAt,
                    Accommodation =
                        b.Room?.Accommodation != null
                            ? new AccommodationSummaryDto
                            {
                                Id = b.Room.Accommodation.Id,
                                Name = b.Room.Accommodation.Name,
                                Description = b.Room.Accommodation.Description,
                                Logo = b.Room.Accommodation.Logo,
                                IsActive = b.Room.Accommodation.IsActive,
                                ActiveRoomCount = 0, // Not needed for booking summary
                            }
                            : null,
                })
                .ToList();

            return new PagedResult<BookingSummaryDto>
            {
                Items = bookingDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };
        }

        public async Task<BookingResponseDto> UpdateBookingStatusAsync(
            int bookingId,
            UpdateBookingStatusDto request
        )
        {
            var booking = await _context
                .Bookings.Include(b => b.Reservation)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new ArgumentException("Booking not found");

            booking.Status = request.Status;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.UpdatedBy = request.UpdatedBy;

            // Handle status-specific logic
            switch (request.Status)
            {
                case BookingStatus.CheckedIn:
                    booking.ActualCheckInAt = DateTime.UtcNow;
                    break;
                case BookingStatus.CheckedOut:
                    booking.ActualCheckOutAt = DateTime.UtcNow;
                    break;
                case BookingStatus.Cancelled:
                    booking.CancelledAt = DateTime.UtcNow;
                    booking.CancelledBy = request.UpdatedBy;
                    booking.CancellationReason = request.Reason;

                    if (booking.Reservation != null)
                    {
                        booking.Reservation.Status = ReservationStatus.Cancelled;
                        booking.Reservation.CancelledAt = DateTime.UtcNow;
                        booking.Reservation.CancellationReason = request.Reason;
                    }
                    break;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Updated booking {BookingReference} status to {Status}",
                booking.BookingReference,
                request.Status
            );

            return await MapToBookingResponseAsync(booking);
        }

        public async Task<bool> ProcessPaymentWebhookAsync(string webhookData, string signature)
        {
            try
            {
                var webhookResult = await _xenditService.ProcessWebhookAsync(
                    webhookData,
                    signature
                );

                if (!webhookResult.IsValid)
                {
                    _logger.LogWarning("Invalid webhook signature received");
                    return false;
                }

                if (string.IsNullOrEmpty(webhookResult.ExternalId))
                {
                    _logger.LogWarning("Webhook missing external ID");
                    return false;
                }

                // Extract booking reference from external ID (format: "booking-{reference}-{timestamp}")
                var parts = webhookResult.ExternalId.Split('-');
                if (parts.Length < 2 || parts[0] != "booking")
                {
                    _logger.LogWarning(
                        "Invalid external ID format: {ExternalId}",
                        webhookResult.ExternalId
                    );
                    return false;
                }

                var bookingReference = parts[1];
                var booking = await _context
                    .Bookings.Include(b => b.Reservation)
                    .Include(b => b.Payments)
                    .FirstOrDefaultAsync(b => b.BookingReference == bookingReference);

                if (booking == null)
                {
                    _logger.LogWarning(
                        "Booking not found for reference: {BookingReference}",
                        bookingReference
                    );
                    return false;
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Handle different webhook events
                    switch (webhookResult.EventType.ToLowerInvariant())
                    {
                        case "invoice.paid":
                            await HandleInvoicePaidWebhook(booking, webhookResult);
                            break;
                        case "invoice.expired":
                            await HandleInvoiceExpiredWebhook(booking, webhookResult);
                            break;
                        case "invoice.payment_failed":
                            await HandleInvoicePaymentFailedWebhook(booking, webhookResult);
                            break;
                        default:
                            _logger.LogInformation(
                                "Unhandled webhook event type: {EventType}",
                                webhookResult.EventType
                            );
                            break;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment webhook");
                return false;
            }
        }

        public async Task<bool> ProcessRefundAsync(int bookingId, RefundPaymentRequestDto request)
        {
            try
            {
                var booking = await _context
                    .Bookings.Include(b => b.Payments)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                    throw new ArgumentException("Booking not found");

                var successfulPayment = booking.Payments.FirstOrDefault(p =>
                    p.Status == PaymentStatus.Paid
                );
                if (successfulPayment == null)
                    throw new InvalidOperationException(
                        "No successful payment found for this booking"
                    );

                var refundAmount = request.RefundAmount ?? successfulPayment.Amount;

                if (refundAmount > successfulPayment.Amount)
                    throw new InvalidOperationException(
                        "Refund amount cannot exceed payment amount"
                    );

                // Create refund through Xendit
                var refundResult = await _xenditService.CreateRefundAsync(
                    successfulPayment.XenditPaymentId ?? "",
                    refundAmount,
                    request.RefundReason
                );

                if (!refundResult.IsSuccess)
                {
                    _logger.LogError(
                        "Failed to create refund for payment {PaymentId}: {Error}",
                        successfulPayment.Id,
                        refundResult.ErrorMessage
                    );
                    return false;
                }

                // Create refund payment record
                var refundPayment = new BookingPayment
                {
                    PaymentReference = $"REF-{DateTime.UtcNow:yyyyMMddHHmmss}-{bookingId}",
                    BookingId = bookingId,
                    PaymentType = PaymentType.Refund,
                    PaymentMethod = successfulPayment.PaymentMethod,
                    Status = PaymentStatus.Processing,
                    Amount = -refundAmount, // Negative amount for refund
                    Currency = successfulPayment.Currency,
                    NetAmount = -refundAmount,
                    RefundedFromPaymentId = successfulPayment.Id,
                    RefundReason = request.RefundReason,
                    CreatedBy = request.ProcessedBy,
                    ProviderMetadata = JsonSerializer.Serialize(refundResult.RawResponse),
                };

                _context.BookingPayments.Add(refundPayment);

                // Update booking payment status if full refund
                if (refundAmount == successfulPayment.Amount)
                {
                    booking.PaymentStatus = PaymentStatus.Refunded;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Initiated refund of {Amount} for booking {BookingReference}",
                    refundAmount,
                    booking.BookingReference
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for booking {BookingId}", bookingId);
                throw;
            }
        }

        public async Task<PagedResult<BookingSummaryDto>> GetUserBookingHistoryAsync(
            int userId,
            int pageNumber = 1,
            int pageSize = 10
        )
        {
            return await SearchBookingsAsync(
                new BookingSearchRequestDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SortBy = "CreatedAt",
                    SortDirection = "DESC",
                },
                userId
            );
        }

        public async Task<List<BookingSummaryDto>> GetUpcomingBookingsAsync(int userId)
        {
            var upcomingBookings = await _context
                .Bookings.Include(b => b.Room)
                .ThenInclude(r => r.Accommodation)
                .Where(b =>
                    b.UserId == userId
                    && b.CheckInDate >= _timezoneService.Today
                    && b.Status != BookingStatus.Cancelled
                )
                .OrderBy(b => b.CheckInDate)
                .Take(10)
                .ToListAsync();

            return upcomingBookings
                .Select(b => new BookingSummaryDto
                {
                    Id = b.Id,
                    BookingReference = b.BookingReference,
                    RoomName = b.Room?.Name ?? "",
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    NumberOfNights = b.NumberOfNights,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status,
                    StatusDescription = GetBookingStatusDescription(b.Status),
                    PaymentStatus = b.PaymentStatus,
                    PaymentStatusDescription = GetPaymentStatusDescription(b.PaymentStatus),
                    GuestName = b.GuestName,
                    CreatedAt = b.CreatedAt,
                    Accommodation =
                        b.Room?.Accommodation != null
                            ? new AccommodationSummaryDto
                            {
                                Id = b.Room.Accommodation.Id,
                                Name = b.Room.Accommodation.Name,
                                Description = b.Room.Accommodation.Description,
                                Logo = b.Room.Accommodation.Logo,
                                IsActive = b.Room.Accommodation.IsActive,
                                ActiveRoomCount = 0,
                            }
                            : null,
                })
                .ToList();
        }

        public async Task<int> CleanupExpiredReservationsAsync()
        {
            try
            {
                var expiredReservations = await _context
                    .BookingReservations.Include(r => r.Booking)
                    .Where(r =>
                        r.Status == ReservationStatus.Active && r.ExpiresAt <= DateTime.UtcNow
                    )
                    .ToListAsync();

                var cleanupCount = 0;

                foreach (var reservation in expiredReservations)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // Update reservation status
                        reservation.Status = ReservationStatus.Expired;
                        reservation.UpdatedAt = DateTime.UtcNow;

                        // Update booking status
                        if (
                            reservation.Booking != null
                            && reservation.Booking.Status == BookingStatus.Reserved
                        )
                        {
                            reservation.Booking.Status = BookingStatus.Cancelled;
                            reservation.Booking.CancelledAt = DateTime.UtcNow;
                            reservation.Booking.CancellationReason = "Reservation expired";
                            reservation.Booking.UpdatedAt = DateTime.UtcNow;
                        }

                        // Release availability lock
                        await ReleaseAvailabilityLockAsync(
                            reservation.BookingId,
                            "Reservation expired"
                        );

                        // Expire Xendit invoice if exists
                        if (!string.IsNullOrEmpty(reservation.XenditInvoiceId))
                        {
                            await _xenditService.ExpireInvoiceAsync(reservation.XenditInvoiceId);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        cleanupCount++;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(
                            ex,
                            "Error cleaning up expired reservation {ReservationId}",
                            reservation.Id
                        );
                    }
                }

                if (cleanupCount > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired reservations", cleanupCount);
                }

                return cleanupCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reservation cleanup");
                return 0;
            }
        }

        public async Task<int> SynchronizePaymentStatusAsync()
        {
            try
            {
                var pendingPayments = await _context
                    .BookingPayments.Where(p =>
                        p.Status == PaymentStatus.Processing
                        && !string.IsNullOrEmpty(p.XenditInvoiceId)
                        && p.CreatedAt > DateTime.UtcNow.AddDays(-7)
                    ) // Only sync payments from last 7 days
                    .ToListAsync();

                var syncCount = 0;

                foreach (var payment in pendingPayments)
                {
                    try
                    {
                        var invoiceStatus = await _xenditService.GetInvoiceStatusAsync(
                            payment.XenditInvoiceId!
                        );

                        var newStatus = invoiceStatus.Status.ToLowerInvariant() switch
                        {
                            "paid" => PaymentStatus.Paid,
                            "expired" => PaymentStatus.Failed,
                            "pending" => PaymentStatus.Pending,
                            _ => payment.Status,
                        };

                        if (newStatus != payment.Status)
                        {
                            payment.Status = newStatus;
                            payment.UpdatedAt = DateTime.UtcNow;

                            if (newStatus == PaymentStatus.Paid)
                            {
                                payment.ConfirmedAt = invoiceStatus.PaidAt ?? DateTime.UtcNow;
                                payment.ProcessedAt = invoiceStatus.PaidAt ?? DateTime.UtcNow;
                                payment.ProviderPaymentMethod = invoiceStatus.PaymentMethod;
                                payment.BankCode = invoiceStatus.BankCode;
                            }

                            syncCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error synchronizing payment {PaymentId}", payment.Id);
                    }
                }

                if (syncCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Synchronized {Count} payment statuses", syncCount);
                }

                return syncCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during payment status synchronization");
                return 0;
            }
        }

        #region Private Helper Methods

        private async Task<(
            bool IsAvailable,
            string Message,
            List<DateTime>? ConflictingDates
        )> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            // First try ledger (cache) - fast path. Ledger is authoritative if present.
            try
            {
                var ledger = await _ledgerService.TryGetMinAvailableFromLedgerAsync(
                    new List<int> { roomId },
                    checkIn,
                    checkOut
                );
                if (ledger != null && ledger.ContainsKey(roomId))
                {
                    var minAvailable = ledger[roomId];
                    if (minAvailable <= 0)
                    {
                        return (false, "Room is not available for the selected dates", null);
                    }
                    else
                    {
                        return (true, "Room is available", null);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ledger read failure: log and continue to fallback checks
                _logger.LogWarning(
                    ex,
                    "Ledger read failed during availability check for room {RoomId}",
                    roomId
                );
            }

            // Check for existing bookings
            var conflictingBookings = await _context
                .Bookings.Where(b =>
                    b.RoomId == roomId
                    && b.Status != BookingStatus.Cancelled
                    && b.CheckInDate < checkOut
                    && b.CheckOutDate > checkIn
                )
                .Select(b => new { b.CheckInDate, b.CheckOutDate })
                .ToListAsync();

            if (conflictingBookings.Any())
            {
                var conflictingDates = new List<DateTime>();
                foreach (var booking in conflictingBookings)
                {
                    for (
                        var date = booking.CheckInDate;
                        date < booking.CheckOutDate;
                        date = date.AddDays(1)
                    )
                    {
                        if (date >= checkIn && date < checkOut && !conflictingDates.Contains(date))
                        {
                            conflictingDates.Add(date);
                        }
                    }
                }

                return (false, "Room is not available for the selected dates", conflictingDates);
            }

            // Check active reservations (including availability locks)
            var activeReservations = await _context
                .BookingReservations.Where(r =>
                    r.RoomId == roomId
                    && r.Status == ReservationStatus.Active
                    && r.ExpiresAt > DateTime.UtcNow
                    && r.CheckInDate < checkOut
                    && r.CheckOutDate > checkIn
                )
                .AnyAsync();

            if (activeReservations)
            {
                return (false, "Room is temporarily reserved by another user", null);
            }

            // Check availability overrides
            var unavailableDates = await _context
                .RoomAvailabilityOverrides.Where(o =>
                    o.RoomId == roomId && !o.IsAvailable && o.Date >= checkIn && o.Date < checkOut
                )
                .Select(o => o.Date)
                .ToListAsync();

            if (unavailableDates.Any())
            {
                return (false, "Room is not available on some selected dates", unavailableDates);
            }

            return (true, "Room is available", null);
        }

        private async Task<BookingPricingDetailsDto> CalculatePricingAsync(
            int roomId,
            DateTime checkIn,
            DateTime checkOut,
            int numberOfGuests
        )
        {
            var numberOfNights = (checkOut - checkIn).Days;
            var dailyPrices = new List<DailyPriceDto>();
            decimal totalBaseAmount = 0;

            // Get room's default price
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null)
                throw new ArgumentException("Room not found");

            var basePrice = room.DefaultPrice;

            // Calculate daily prices with modifiers
            for (var date = checkIn; date < checkOut; date = date.AddDays(1))
            {
                var dailyPrice = await _roomCalendarService.GetPriceForDateAsync(roomId, date);
                var isWeekend =
                    date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                var isHoliday = await IsHolidayAsync(date);

                var priceModifiers = new List<string>();
                if (isWeekend)
                    priceModifiers.Add("Weekend");
                if (isHoliday)
                    priceModifiers.Add("Holiday");

                dailyPrices.Add(
                    new DailyPriceDto
                    {
                        Date = date,
                        Price = dailyPrice,
                        IsWeekend = isWeekend,
                        IsHoliday = isHoliday,
                        PriceModifiers = priceModifiers,
                    }
                );

                totalBaseAmount += dailyPrice;
            }

            // Calculate taxes and fees
            var taxRate = _config.TaxRate;
            var serviceFeeRate = _config.ServiceFeeRate;

            var taxAmount = totalBaseAmount * taxRate;
            var serviceFee = totalBaseAmount * serviceFeeRate;
            var totalAmount = totalBaseAmount + taxAmount + serviceFee;

            return new BookingPricingDetailsDto
            {
                BasePrice = basePrice,
                BaseAmount = totalBaseAmount,
                TaxAmount = taxAmount,
                TaxRate = taxRate,
                ServiceFee = serviceFee,
                ServiceFeeRate = serviceFeeRate,
                TotalAmount = totalAmount,
                NumberOfNights = numberOfNights,
                DailyPrices = dailyPrices,
                AppliedPromotions = new List<string>(), // TODO: Implement promotions
            };
        }

        private async Task<bool> IsHolidayAsync(DateTime date)
        {
            return await _context.HolidayCalendar.AnyAsync(h => h.Date == date && h.IsActive);
        }

        private async Task<string> GenerateBookingReferenceAsync()
        {
            string bookingRef;
            bool exists;

            do
            {
                var timestamp = DateTimeOffset.UtcNow.ToString("yyMMdd");
                var random = new Random().Next(1000, 9999);
                bookingRef = $"VB-{timestamp}{random}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

                exists = await _context.Bookings.AnyAsync(b => b.BookingReference == bookingRef);
            } while (exists);

            return bookingRef;
        }

        private async Task ReleaseAvailabilityLockAsync(int bookingId, string reason)
        {
            var locks = await _context
                .BookingAvailabilityLocks.Where(l => l.BookingId == bookingId && l.IsActive)
                .ToListAsync();

            foreach (var lockItem in locks)
            {
                lockItem.IsActive = false;
                lockItem.ReleasedAt = DateTime.UtcNow;
                lockItem.ReleaseReason = reason;
            }
        }

        private async Task HandleInvoicePaidWebhook(
            Booking booking,
            XenditWebhookResult webhookResult
        )
        {
            // Create payment record
            var payment = new BookingPayment
            {
                PaymentReference = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{booking.Id}",
                BookingId = booking.Id,
                PaymentType = PaymentType.FullPayment,
                PaymentMethod = webhookResult.PaymentMethod ?? PaymentMethod.CreditCard,
                Status = PaymentStatus.Paid,
                Amount = webhookResult.Amount ?? booking.TotalAmount,
                Currency = "USD", // TODO: Make configurable
                NetAmount = webhookResult.Amount ?? booking.TotalAmount,
                XenditInvoiceId = webhookResult.InvoiceId,
                XenditPaymentId = webhookResult.PaymentId,
                ProcessedAt = webhookResult.PaidAt ?? DateTime.UtcNow,
                ConfirmedAt = webhookResult.PaidAt ?? DateTime.UtcNow,
                XenditWebhookData = JsonSerializer.Serialize(webhookResult.WebhookData),
            };

            _context.BookingPayments.Add(payment);

            // Confirm booking
            await ConfirmBookingAsync(booking.Id, webhookResult.PaymentId ?? "");
        }

        private async Task HandleInvoiceExpiredWebhook(
            Booking booking,
            XenditWebhookResult webhookResult
        )
        {
            if (booking.Status == BookingStatus.Reserved)
            {
                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;
                booking.CancellationReason = "Payment expired";
                booking.UpdatedAt = DateTime.UtcNow;

                if (booking.Reservation != null)
                {
                    booking.Reservation.Status = ReservationStatus.Expired;
                    booking.Reservation.UpdatedAt = DateTime.UtcNow;
                }

                await ReleaseAvailabilityLockAsync(booking.Id, "Payment expired");
            }
        }

        private async Task HandleInvoicePaymentFailedWebhook(
            Booking booking,
            XenditWebhookResult webhookResult
        )
        {
            // Log payment failure but don't automatically cancel the booking
            _logger.LogWarning(
                "Payment failed for booking {BookingReference}: {Reason}",
                booking.BookingReference,
                webhookResult.ErrorMessage
            );

            // Create failed payment record
            var payment = new BookingPayment
            {
                PaymentReference = $"FAIL-{DateTime.UtcNow:yyyyMMddHHmmss}-{booking.Id}",
                BookingId = booking.Id,
                PaymentType = PaymentType.FullPayment,
                PaymentMethod = webhookResult.PaymentMethod ?? PaymentMethod.CreditCard,
                Status = PaymentStatus.Failed,
                Amount = webhookResult.Amount ?? booking.TotalAmount,
                Currency = "USD",
                NetAmount = 0,
                XenditInvoiceId = webhookResult.InvoiceId,
                FailedAt = DateTime.UtcNow,
                FailureReason = webhookResult.ErrorMessage ?? "Payment failed",
                XenditWebhookData = JsonSerializer.Serialize(webhookResult.WebhookData),
            };

            _context.BookingPayments.Add(payment);
            await _context.SaveChangesAsync();
        }

        private async Task<BookingResponseDto> MapToBookingResponseAsync(Booking booking)
        {
            // Ensure all required navigation properties are loaded
            if (booking.Room == null)
            {
                booking = await _context
                    .Bookings.Include(b => b.Room)
                    .ThenInclude(r => r.Photos)
                    .Include(b => b.Room)
                    .ThenInclude(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Accommodation)
                    .Include(b => b.User)
                    .Include(b => b.Reservation)
                    .Include(b => b.Payments)
                    .FirstAsync(b => b.Id == booking.Id);
            }

            return new BookingResponseDto
            {
                Id = booking.Id,
                BookingReference = booking.BookingReference,
                RoomId = booking.RoomId,
                RoomName = booking.Room?.Name ?? "",
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                NumberOfGuests = booking.NumberOfGuests,
                NumberOfNights = booking.NumberOfNights,
                BaseAmount = booking.BaseAmount,
                TaxAmount = booking.TaxAmount,
                ServiceFee = booking.ServiceFee,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status,
                PaymentStatus = booking.PaymentStatus,
                GuestName = booking.GuestName,
                GuestEmail = booking.GuestEmail,
                GuestPhone = booking.GuestPhone,
                SpecialRequests = booking.SpecialRequests,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,
                CancelledAt = booking.CancelledAt,
                CancellationReason = booking.CancellationReason,
                ActualCheckInAt = booking.ActualCheckInAt,
                ActualCheckOutAt = booking.ActualCheckOutAt,
                Reservation =
                    booking.Reservation != null ? MapToReservationDto(booking.Reservation) : null,
                Payments = booking.Payments.Select(MapToPaymentDto).ToList(),
                Room =
                    booking.Room != null
                        ? new RoomSummaryDto
                        {
                            Id = booking.Room.Id,
                            Name = booking.Room.Name,
                            Description = booking.Room.Description,
                            DefaultPrice = booking.Room.DefaultPrice,
                            MaxGuests = booking.Room.MaxGuests,
                            MainPhotoUrl = booking.Room.MainPhotoUrl,
                            Amenities = booking.Room.Amenities.Select(a => a.Name).ToList(),
                        }
                        : null,
                Accommodation =
                    booking.Room?.Accommodation != null
                        ? new AccommodationSummaryDto
                        {
                            Id = booking.Room.Accommodation.Id,
                            Name = booking.Room.Accommodation.Name,
                            Description = booking.Room.Accommodation.Description,
                            Logo = booking.Room.Accommodation.Logo,
                            IsActive = booking.Room.Accommodation.IsActive,
                            ActiveRoomCount = 0, // Not needed for booking response
                        }
                        : null,
            };
        }

        private BookingReservationDto MapToReservationDto(BookingReservation reservation)
        {
            return new BookingReservationDto
            {
                Id = reservation.Id,
                ReservationReference = reservation.ReservationReference,
                Status = reservation.Status,
                ReservedAt = reservation.ReservedAt,
                ExpiresAt = reservation.ExpiresAt,
                PaymentUrl = reservation.PaymentUrl,
                PaymentUrlExpiresAt = reservation.PaymentUrlExpiresAt,
                ExtensionCount = reservation.ExtensionCount,
                CanExtend = reservation.CanExtend,
                IsActive = reservation.IsActive,
                IsExpired = reservation.IsExpired,
                TimeUntilExpiry = reservation.TimeUntilExpiry,
            };
        }

        private BookingPaymentDto MapToPaymentDto(BookingPayment payment)
        {
            return new BookingPaymentDto
            {
                Id = payment.Id,
                PaymentReference = payment.PaymentReference,
                PaymentType = payment.PaymentType,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                Amount = payment.Amount,
                Currency = payment.Currency,
                NetAmount = payment.NetAmount,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                ConfirmedAt = payment.ConfirmedAt,
                FailureReason = payment.FailureReason,
                CardLastFour = payment.CardLastFour,
                BankCode = payment.BankCode,
                PaymentDescription = payment.PaymentDescription,
            };
        }

        private static string GetBookingStatusDescription(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Reserved => "Reserved",
                BookingStatus.Confirmed => "Confirmed",
                BookingStatus.CheckedIn => "Checked In",
                BookingStatus.CheckedOut => "Checked Out",
                BookingStatus.Cancelled => "Cancelled",
                BookingStatus.NoShow => "No Show",
                _ => "Unknown",
            };
        }

        private static string GetPaymentStatusDescription(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "Pending",
                PaymentStatus.Processing => "Processing",
                PaymentStatus.Paid => "Paid",
                PaymentStatus.Failed => "Failed",
                PaymentStatus.Refunded => "Refunded",
                PaymentStatus.PartialRefund => "Partially Refunded",
                _ => "Unknown",
            };
        }

        /// <summary>
        /// Gets the current GMT+8 time and converts it to UTC for database storage
        /// </summary>
        private DateTime GetCurrentUtcTime()
        {
            return _timezoneService.ConvertToUtc(_timezoneService.Now);
        }

        #endregion
    }

    public class BookingConfiguration
    {
        public int DefaultReservationTimeoutMinutes { get; set; } = 30;
        public int MaxReservationExtensions { get; set; } = 2;
        public int ReservationExtensionMinutes { get; set; } = 15;
        public decimal TaxRate { get; set; } = 0.10m; // 10%
        public decimal ServiceFeeRate { get; set; } = 0.07m; // 7%
        public bool AllowSameDayBooking { get; set; } = true;
        public int MaxAdvanceBookingDays { get; set; } = 365;
        public bool RequirePaymentForReservation { get; set; } = true;
        public TimeSpan CheckInTime { get; set; } = TimeSpan.FromHours(15); // 3:00 PM
        public TimeSpan CheckOutTime { get; set; } = TimeSpan.FromHours(11); // 11:00 AM
    }
}
