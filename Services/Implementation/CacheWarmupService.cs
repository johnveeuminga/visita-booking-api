using Microsoft.EntityFrameworkCore;
using VisitaBookingApi.Data;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.Cache;

namespace visita_booking_api.Services.Implementation
{
    public class CacheWarmupService : ICacheWarmupService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheWarmupService> _logger;

        public CacheWarmupService(
            ApplicationDbContext context,
            ICacheService cacheService,
            ILogger<CacheWarmupService> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task WarmupRoomCacheAsync(int roomId)
        {
            try
            {
                // Only cache search-related data, not complex entities
                // Check if room exists and is active
                var roomExists = await _context.Rooms
                    .AnyAsync(r => r.Id == roomId && r.IsActive);

                if (roomExists)
                {
                    // For now, we're only caching search results and price data
                    // Room entities themselves are fetched directly from database
                    _logger.LogDebug("Room {RoomId} exists and is available for caching search data", roomId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room {RoomId} for cache warmup", roomId);
            }
        }

        public async Task WarmupPopularRoomsAsync()
        {
            try
            {
                // Get top 10 most recently updated rooms as "popular" for now
                var popularRoomIds = await _context.Rooms
                    .Where(r => r.IsActive)
                    .OrderByDescending(r => r.UpdatedAt)
                    .Take(10)
                    .Select(r => r.Id)
                    .ToListAsync();

                // Process sequentially to avoid DbContext concurrency issues
                foreach (var roomId in popularRoomIds)
                {
                    await WarmupRoomCacheAsync(roomId);
                }

                _logger.LogInformation("Warmed up cache for {Count} popular rooms", popularRoomIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up popular rooms cache");
            }
        }

        public async Task WarmupAmenitiesCacheAsync()
        {
            try
            {
                // Simplified: Not caching amenities to avoid circular reference issues
                // Amenities are fetched directly from database when needed
                var amenitiesCount = await _context.Amenities
                    .Where(a => a.IsActive)
                    .CountAsync();

                _logger.LogDebug("Skipping amenities cache - {Count} amenities available in database", amenitiesCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking amenities for cache warmup");
            }
        }

        public async Task WarmupAvailabilityCacheAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // Simplified: We're not caching availability data for now
                // Availability checks are done directly against the database using exclusion queries
                _logger.LogDebug("Skipping availability cache warmup for room {RoomId} - using direct DB queries", roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in availability cache warmup for room {RoomId}", roomId);
            }
        }
    }

    public class CacheWarmupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CacheWarmupBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public CacheWarmupBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<CacheWarmupBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initial warmup after startup delay
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            
            if (!stoppingToken.IsCancellationRequested)
            {
                await InitialWarmup();
            }

            // Periodic warmup every 2 hours
            var timer = new PeriodicTimer(TimeSpan.FromHours(2));
            
            try
            {
                while (!stoppingToken.IsCancellationRequested && 
                       await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await PeriodicWarmup();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Cache warmup background service was cancelled");
            }
            finally
            {
                timer?.Dispose();
            }
        }

        private async Task InitialWarmup()
        {
            try
            {
                _logger.LogInformation("Starting initial cache warmup");
                
                using var scope = _serviceProvider.CreateScope();
                var warmupService = scope.ServiceProvider.GetRequiredService<ICacheWarmupService>();

                // Warmup essential data
                await warmupService.WarmupAmenitiesCacheAsync();
                await warmupService.WarmupPopularRoomsAsync();

                _logger.LogInformation("Initial cache warmup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during initial cache warmup");
            }
        }

        private async Task PeriodicWarmup()
        {
            try
            {
                _logger.LogInformation("Starting periodic cache warmup");
                
                using var scope = _serviceProvider.CreateScope();
                var warmupService = scope.ServiceProvider.GetRequiredService<ICacheWarmupService>();

                // Warmup popular rooms and amenities
                await warmupService.WarmupPopularRoomsAsync();
                await warmupService.WarmupAmenitiesCacheAsync();

                _logger.LogInformation("Periodic cache warmup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during periodic cache warmup");
            }
        }
    }
}