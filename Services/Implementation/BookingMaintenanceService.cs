using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Services.Implementation
{
    /// <summary>
    /// Background service that handles periodic cleanup of expired reservations
    /// and synchronization of payment statuses with Xendit
    /// </summary>
    public class BookingMaintenanceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingMaintenanceService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Run every 5 minutes
        private readonly TimeSpan _paymentSyncInterval = TimeSpan.FromMinutes(15); // Run every 15 minutes

        public BookingMaintenanceService(
            IServiceProvider serviceProvider,
            ILogger<BookingMaintenanceService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Maintenance Service started");

            var lastCleanupTime = DateTime.UtcNow;
            var lastPaymentSyncTime = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;

                    // Check if it's time to run cleanup
                    if (now - lastCleanupTime >= _cleanupInterval)
                    {
                        await PerformReservationCleanupAsync();
                        lastCleanupTime = now;
                    }

                    // Check if it's time to run payment synchronization
                    if (now - lastPaymentSyncTime >= _paymentSyncInterval)
                    {
                        await PerformPaymentSynchronizationAsync();
                        lastPaymentSyncTime = now;
                    }

                    // Wait for a shorter interval to check timing conditions more frequently
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Booking Maintenance Service");
                    // Continue running even if there's an error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Booking Maintenance Service stopped");
        }

        private async Task PerformReservationCleanupAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                var cleanedCount = await bookingService.CleanupExpiredReservationsAsync();
                
                if (cleanedCount > 0)
                {
                    _logger.LogInformation("Reservation cleanup completed. Cleaned up {Count} expired reservations", cleanedCount);
                }
                else
                {
                    _logger.LogDebug("Reservation cleanup completed. No expired reservations found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reservation cleanup");
            }
        }

        private async Task PerformPaymentSynchronizationAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                var syncedCount = await bookingService.SynchronizePaymentStatusAsync();
                
                if (syncedCount > 0)
                {
                    _logger.LogInformation("Payment synchronization completed. Synchronized {Count} payments", syncedCount);
                }
                else
                {
                    _logger.LogDebug("Payment synchronization completed. No payments to synchronize");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during payment synchronization");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Booking Maintenance Service is stopping...");
            await base.StopAsync(cancellationToken);
        }
    }
}