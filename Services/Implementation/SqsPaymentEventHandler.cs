using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;
using VisitaBookingApi.Services.Interfaces;

namespace visita_booking_api.Services.Implementation
{
    /// <summary>
    /// Handles payment events from SQS. This implementation assumes the payment ExternalId contains booking reference
    /// in the form "booking-{bookingReference}-{timestamp}" similar to existing webhook logic used for Xendit.
    /// The handler is idempotent and will skip already processed payments.
    /// </summary>
    public class SqsPaymentEventHandler : IPaymentEventHandler
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookingService _bookingService;
        private readonly IAvailabilityLedgerService _ledgerService;
        private readonly ICacheInvalidationService _cacheInvalidation;
        private readonly ILogger<SqsPaymentEventHandler> _logger;

        public SqsPaymentEventHandler(
            ApplicationDbContext context,
            IBookingService bookingService,
            IAvailabilityLedgerService ledgerService,
            ICacheInvalidationService cacheInvalidation,
            ILogger<SqsPaymentEventHandler> logger)
        {
            _context = context;
            _bookingService = bookingService;
            _ledgerService = ledgerService;
            _cacheInvalidation = cacheInvalidation;
            _logger = logger;
        }

        public async Task HandleAsync(PaymentEventDto paymentEvent, CancellationToken cancellationToken = default)
        {
            if (paymentEvent == null)
                throw new ArgumentNullException(nameof(paymentEvent));

            _logger.LogInformation("Processing payment event Id={Id} ExternalId={ExternalId} Status={Status}",
                paymentEvent.Id, paymentEvent.ExternalId, paymentEvent.Status);

            // Basic validation
            if (string.IsNullOrEmpty(paymentEvent.ExternalId))
            {
                _logger.LogWarning("Payment event missing ExternalId; skipping");
                return;
            }

            // Parse booking reference from external id. Reuse same logic as BookingService.ProcessPaymentWebhookAsync
            var parts = paymentEvent.ExternalId.Split('-');
            if (parts.Length < 2 || parts[0] != "booking")
            {
                _logger.LogWarning("Unsupported ExternalId format: {ExternalId}", paymentEvent.ExternalId);
                return;
            }

            var bookingReference = parts[1];

            // Load booking and check idempotency
            var booking = await _context.Bookings
                .Include(b => b.Payments)
                .Include(b => b.Reservation)
                .FirstOrDefaultAsync(b => b.BookingReference == bookingReference, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning("Booking not found for reference {Ref}; skipping payment event", bookingReference);
                return;
            }

            // Check if payment with same provider external id already exists
            var existing = booking.Payments.FirstOrDefault(p => p.XenditExternalId == paymentEvent.ExternalId || p.ProviderTransactionId == paymentEvent.Id);
            if (existing != null && existing.IsSuccessful)
            {
                _logger.LogInformation("Payment already processed for booking {Ref} payment id {PaymentId}", bookingReference, existing.Id);
                return; // idempotent
            }

            // Map event to BookingPayment
            var payment = new BookingPayment
            {
                PaymentReference = $"VBPAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{booking.Id}",
                BookingId = booking.Id,
                PaymentType = PaymentType.FullPayment,
                PaymentMethod = PaymentMethod.BankTransfer,
                Status = paymentEvent.Status?.ToLowerInvariant() == "paid" ? PaymentStatus.Paid : PaymentStatus.Processing,
                Amount = booking.TotalAmount,
                Currency = "PHP",
                XenditExternalId = paymentEvent.ExternalId,
                ProviderTransactionId = paymentEvent.Id,
                ProcessedAt = DateTime.UtcNow,
                ConfirmedAt = paymentEvent.Status?.ToLowerInvariant() == "paid" ? DateTime.UtcNow : null,
                ProviderMetadata = JsonSerializer.Serialize(paymentEvent)
            };

            // Save payment and, if paid, confirm booking through BookingService which will release ledger locks
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _context.BookingPayments.Add(payment);
                await _context.SaveChangesAsync(cancellationToken);

                if (payment.Status == PaymentStatus.Paid)
                {
                    // Confirm booking using BookingService so existing invariants (ledger releases) are preserved
                    await _bookingService.ConfirmBookingAsync(booking.Id, payment.ProviderTransactionId ?? "");
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error saving payment for booking {Ref}", bookingReference);
                throw;
            }

            // Invalidate caches for this room so availability/pricing reflect the confirmed booking
            try
            {
                if (booking.RoomId > 0)
                {
                    await _cacheInvalidation.InvalidateRoomCacheAsync(booking.RoomId, CacheInvalidationType.BookingCreated);

                    // Warmup ledger for affected room/date range so ledger-backed searches pick up the change
                    try
                    {
                        var startDate = booking.CheckInDate.Date;
                        var endDate = booking.CheckOutDate.Date;
                        await _ledgerService.WarmupRoomLedgerAsync(booking.RoomId, startDate, endDate);
                        // Also explicitly invalidate availability cache pattern to ensure consumers refresh
                        await _cacheInvalidation.InvalidateAvailabilityCacheAsync(booking.RoomId);
                    }
                    catch (Exception lex)
                    {
                        _logger.LogWarning(lex, "Failed to warmup ledger for room {RoomId} after payment for booking {Ref}", booking.RoomId, bookingReference);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache after processing payment for booking {Ref}", bookingReference);
            }
        }
    }
}
