using Microsoft.EntityFrameworkCore;
using VisitaBookingApi.Data;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.Entities;
using visita_booking_api.Models.DTOs;

namespace visita_booking_api.Services.Implementation
{
    public class RoomPriceCacheService : IRoomPriceCacheService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomPriceCacheService> _logger;

        public RoomPriceCacheService(
            ApplicationDbContext context,
            ILogger<RoomPriceCacheService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpdateRoomPriceCacheAsync(int roomId)
        {
            try
            {
                _logger.LogInformation("Updating price cache for room {RoomId}", roomId);

                // Check if room exists
                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId && r.IsActive);
                if (room == null)
                {
                    _logger.LogWarning("Room {RoomId} not found or inactive", roomId);
                    return;
                }

                // Calculate price ranges using pricing rules
                var now = DateTime.UtcNow;
                var thirtyDaysOut = now.AddDays(30);
                var ninetyDaysOut = now.AddDays(90);

                // Get pricing rules for this room
                var pricingRules = await _context.RoomPricingRules
                    .Where(r => r.RoomId == roomId && r.IsActive)
                    .OrderBy(r => r.Priority)
                    .ToListAsync();

                // Calculate prices for the next 30 and 90 days
                var thirtyDayPrices = new List<decimal>();
                var ninetyDayPrices = new List<decimal>();

                for (int i = 0; i < 90; i++)
                {
                    var date = now.AddDays(i);
                    var dayPrice = CalculatePriceForDate(room.DefaultPrice, date, pricingRules);
                    
                    ninetyDayPrices.Add(dayPrice);
                    if (i < 30)
                    {
                        thirtyDayPrices.Add(dayPrice);
                    }
                }

                // Calculate multipliers
                var weekendMultiplier = CalculateWeekendMultiplier(pricingRules);
                var holidayMultiplier = CalculateHolidayMultiplier(pricingRules);
                var peakSeasonMultiplier = CalculatePeakSeasonMultiplier(pricingRules);

                // Determine price band
                var avgPrice = ninetyDayPrices.Average();
                var priceBand = DeterminePriceBand(avgPrice);

                // Update or create cache entry
                var cacheEntry = await _context.RoomPriceCaches
                    .FirstOrDefaultAsync(c => c.RoomId == roomId);

                if (cacheEntry == null)
                {
                    cacheEntry = new RoomPriceCache
                    {
                        RoomId = roomId,
                        CreatedAt = now
                    };
                    _context.RoomPriceCaches.Add(cacheEntry);
                }

                // Update cache values
                cacheEntry.MinPrice30Days = thirtyDayPrices.Min();
                cacheEntry.MaxPrice30Days = thirtyDayPrices.Max();
                cacheEntry.AvgPrice30Days = thirtyDayPrices.Average();
                cacheEntry.MinPrice90Days = ninetyDayPrices.Min();
                cacheEntry.MaxPrice90Days = ninetyDayPrices.Max();
                cacheEntry.AvgPrice90Days = ninetyDayPrices.Average();
                cacheEntry.WeekendMultiplier = weekendMultiplier;
                cacheEntry.HolidayMultiplier = holidayMultiplier;
                cacheEntry.PeakSeasonMultiplier = peakSeasonMultiplier;
                cacheEntry.PriceBand = priceBand;
                cacheEntry.LastUpdated = now;
                cacheEntry.LastPricingRuleChange = now;
                cacheEntry.DataValidUntil = now.AddDays(7); // Cache valid for 7 days
                cacheEntry.UpdatedAt = now;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully updated price cache for room {RoomId}", roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating price cache for room {RoomId}", roomId);
                throw;
            }
        }

        public async Task UpdateRoomsPriceCacheAsync(List<int> roomIds)
        {
            _logger.LogInformation("Updating price cache for {Count} rooms", roomIds.Count);

            // Run sequentially to avoid concurrent DbContext access
            foreach (var roomId in roomIds)
            {
                try
                {
                    await UpdateRoomPriceCacheAsync(roomId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update price cache for room {RoomId}", roomId);
                    // Continue with other rooms even if one fails
                }
            }

            _logger.LogInformation("Completed price cache updates for {Count} rooms", roomIds.Count);
        }

        public async Task<RoomPriceCache?> GetRoomPriceCacheAsync(int roomId)
        {
            return await _context.RoomPriceCaches
                .Include(c => c.Room)
                .FirstOrDefaultAsync(c => c.RoomId == roomId && c.DataValidUntil > DateTime.UtcNow);
        }

        public async Task<List<int>> FilterRoomIdsByPriceRangeAsync(
            decimal? minPrice,
            decimal? maxPrice,
            DateTime checkIn,
            DateTime checkOut,
            List<int>? roomIds = null)
        {
            try
            {
                var query = _context.RoomPriceCaches.AsQueryable();

                // Filter by room IDs if provided
                if (roomIds?.Any() == true)
                {
                    query = query.Where(c => roomIds.Contains(c.RoomId));
                }

                // Only include valid cache entries
                query = query.Where(c => c.DataValidUntil > DateTime.UtcNow);

                // Apply price range filters
                if (minPrice.HasValue)
                {
                    query = query.Where(c => c.MaxPrice90Days >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(c => c.MinPrice90Days <= maxPrice.Value);
                }

                var result = await query.Select(c => c.RoomId).ToListAsync();
                
                _logger.LogDebug("Price range filter returned {Count} room IDs", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering rooms by price range");
                return new List<int>();
            }
        }

        /// <summary>
        /// Get room IDs that should be EXCLUDED from search results based on price range
        /// This is more efficient when most rooms match the criteria
        /// </summary>
        public async Task<List<int>> GetRoomIdsToExcludeByPriceRangeAsync(
            decimal? minPrice,
            decimal? maxPrice,
            DateTime checkIn,
            DateTime checkOut,
            List<int>? roomIds = null)
        {
            try
            {
                var query = _context.RoomPriceCaches.AsQueryable();

                // Filter by room IDs if provided
                if (roomIds?.Any() == true)
                {
                    query = query.Where(c => roomIds.Contains(c.RoomId));
                }

                // Only include valid cache entries
                query = query.Where(c => c.DataValidUntil > DateTime.UtcNow);

                // Find rooms that DON'T match the price criteria (exclusion logic)
                var exclusionQuery = query.Where(c => false); // Start with empty query

                if (minPrice.HasValue)
                {
                    // Exclude rooms where max price is below the minimum
                    var tooLowQuery = query.Where(c => c.MaxPrice90Days < minPrice.Value);
                    exclusionQuery = exclusionQuery.Union(tooLowQuery);
                }

                if (maxPrice.HasValue)
                {
                    // Exclude rooms where min price is above the maximum
                    var tooHighQuery = query.Where(c => c.MinPrice90Days > maxPrice.Value);
                    exclusionQuery = exclusionQuery.Union(tooHighQuery);
                }

                var excludeIds = await exclusionQuery.Select(c => c.RoomId).Distinct().ToListAsync();
                
                _logger.LogDebug("Price range exclusion filter found {Count} room IDs to exclude", excludeIds.Count);
                return excludeIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room IDs to exclude by price range");
                return new List<int>();
            }
        }

        public async Task<(decimal EstimatedMin, decimal EstimatedMax)> EstimatePriceRangeAsync(
            int roomId,
            DateTime checkIn,
            DateTime checkOut)
        {
            try
            {
                var cache = await GetRoomPriceCacheAsync(roomId);
                if (cache == null)
                {
                    _logger.LogWarning("No valid cache found for room {RoomId}", roomId);
                    var room = await _context.Rooms.FindAsync(roomId);
                    return room != null ? (room.DefaultPrice, room.DefaultPrice) : (0, 0);
                }

                // Use 30-day or 90-day range depending on date range
                var days = (checkOut - checkIn).Days;
                var baseMin = days <= 30 ? cache.MinPrice30Days : cache.MinPrice90Days;
                var baseMax = days <= 30 ? cache.MaxPrice30Days : cache.MaxPrice90Days;

                // Apply seasonal multipliers based on check-in date
                var multiplier = GetSeasonalMultiplier(checkIn, cache);
                
                return (baseMin * multiplier, baseMax * multiplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating price range for room {RoomId}", roomId);
                return (0, 0);
            }
        }

        public async Task InvalidatePriceCacheAsync(List<int> roomIds)
        {
            try
            {
                _logger.LogInformation("Invalidating price cache for {Count} rooms", roomIds.Count);

                var cacheEntries = await _context.RoomPriceCaches
                    .Where(c => roomIds.Contains(c.RoomId))
                    .ToListAsync();

                foreach (var entry in cacheEntries)
                {
                    entry.DataValidUntil = DateTime.UtcNow.AddMinutes(-1); // Mark as expired
                    entry.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Invalidated {Count} cache entries", cacheEntries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating price cache");
                throw;
            }
        }

        public async Task RefreshStalePriceCachesAsync(int batchSize = 100)
        {
            try
            {
                _logger.LogInformation("Refreshing stale price caches (batch size: {BatchSize})", batchSize);

                var staleCaches = await _context.RoomPriceCaches
                    .Where(c => c.DataValidUntil <= DateTime.UtcNow)
                    .Take(batchSize)
                    .Select(c => c.RoomId)
                    .ToListAsync();

                if (staleCaches.Any())
                {
                    await UpdateRoomsPriceCacheAsync(staleCaches);
                    _logger.LogInformation("Refreshed {Count} stale cache entries", staleCaches.Count);
                }
                else
                {
                    _logger.LogInformation("No stale cache entries found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing stale price caches");
                throw;
            }
        }

        public async Task RebuildAllPriceCachesAsync()
        {
            try
            {
                _logger.LogInformation("Starting rebuild of all price caches");

                var activeRoomIds = await _context.Rooms
                    .Where(r => r.IsActive)
                    .Select(r => r.Id)
                    .ToListAsync();

                // Clear existing cache
                var existingCaches = await _context.RoomPriceCaches.ToListAsync();
                _context.RoomPriceCaches.RemoveRange(existingCaches);
                await _context.SaveChangesAsync();

                // Rebuild all caches
                await UpdateRoomsPriceCacheAsync(activeRoomIds);

                _logger.LogInformation("Completed rebuilding all price caches for {Count} rooms", activeRoomIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding all price caches");
                throw;
            }
        }

        public async Task<PriceCacheStatsDTO> GetCacheStatsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var caches = await _context.RoomPriceCaches.ToListAsync();

                var validCaches = caches.Where(c => c.DataValidUntil > now).ToList();
                var staleCaches = caches.Where(c => c.DataValidUntil <= now).ToList();

                var stats = new PriceCacheStatsDTO
                {
                    TotalCacheEntries = caches.Count,
                    ValidCacheEntries = validCaches.Count,
                    StaleCacheEntries = staleCaches.Count,
                    ExpiredCacheEntries = staleCaches.Count, // Same as stale in this implementation
                    OldestCacheEntry = caches.Any() ? caches.Min(c => c.CreatedAt) : null,
                    NewestCacheEntry = caches.Any() ? caches.Max(c => c.UpdatedAt) : null,
                    CacheHitRatio = caches.Any() ? (double)caches.Sum(c => c.SearchHitCount) / Math.Max(1, caches.Count) : 0.0,
                    TotalSearchHits = caches.Sum(c => c.SearchHitCount),
                    AverageMinPrice = validCaches.Any() ? validCaches.Average(c => c.MinPrice30Days) : 0,
                    AverageMaxPrice = validCaches.Any() ? validCaches.Average(c => c.MaxPrice30Days) : 0,
                    PriceBandDistribution = validCaches.GroupBy(c => c.PriceBand.ToString())
                        .ToDictionary(g => g.Key, g => g.Count()),
                    LastRefreshTime = caches.Any() ? caches.Max(c => c.LastUpdated) : DateTime.MinValue
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                throw;
            }
        }

        #region Private Helper Methods

        private decimal CalculatePriceForDate(decimal basePrice, DateTime date, List<RoomPricingRule> rules)
        {
            var price = basePrice;

            foreach (var rule in rules)
            {
                if (IsRuleApplicable(rule, date))
                {
                    // RoomPricingRule uses fixed price only
                    price = rule.FixedPrice;
                }
            }

            return price;
        }

        private bool IsRuleApplicable(RoomPricingRule rule, DateTime date)
        {
            // Check date range
            if (rule.StartDate.HasValue && date < rule.StartDate.Value) return false;
            if (rule.EndDate.HasValue && date > rule.EndDate.Value) return false;

            // Check day of week
            if (rule.DayOfWeek.HasValue && (int)date.DayOfWeek != rule.DayOfWeek.Value) return false;

            return true;
        }

        private decimal CalculateWeekendMultiplier(List<RoomPricingRule> rules)
        {
            var weekendRules = rules.Where(r => r.RuleType == PricingRuleType.Weekend && r.DayOfWeek.HasValue && 
                (r.DayOfWeek.Value == 0 || r.DayOfWeek.Value == 6)); // Saturday or Sunday

            if (!weekendRules.Any()) return 1.0m;
            
            // Calculate multiplier based on average fixed price vs base price
            var avgFixedPrice = weekendRules.Average(r => r.FixedPrice);
            // We need a base price to calculate multiplier - use a reasonable default
            var estimatedBasePrice = 100m; // This is a simplification
            return avgFixedPrice > 0 ? avgFixedPrice / estimatedBasePrice : 1.0m;
        }

        private decimal CalculateHolidayMultiplier(List<RoomPricingRule> rules)
        {
            var holidayRules = rules.Where(r => r.RuleType == PricingRuleType.Holiday);
            if (!holidayRules.Any()) return 1.0m;
            
            var avgFixedPrice = holidayRules.Average(r => r.FixedPrice);
            var estimatedBasePrice = 100m;
            return avgFixedPrice > 0 ? avgFixedPrice / estimatedBasePrice : 1.0m;
        }

        private decimal CalculatePeakSeasonMultiplier(List<RoomPricingRule> rules)
        {
            var peakSeasonRules = rules.Where(r => r.RuleType == PricingRuleType.Seasonal);
            if (!peakSeasonRules.Any()) return 1.0m;
            
            var avgFixedPrice = peakSeasonRules.Average(r => r.FixedPrice);
            var estimatedBasePrice = 100m;
            return avgFixedPrice > 0 ? avgFixedPrice / estimatedBasePrice : 1.0m;
        }

        private PriceBand DeterminePriceBand(decimal avgPrice)
        {
            // Simple price banding logic - this could be made more sophisticated
            if (avgPrice < 50) return PriceBand.Budget;
            if (avgPrice < 100) return PriceBand.Economy;
            if (avgPrice < 200) return PriceBand.Standard;
            if (avgPrice < 400) return PriceBand.Premium;
            return PriceBand.Luxury;
        }

        private decimal GetSeasonalMultiplier(DateTime date, RoomPriceCache cache)
        {
            var multiplier = 1.0m;

            // Weekend check
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                multiplier *= cache.WeekendMultiplier;
            }

            // Holiday check would require holiday calendar lookup
            // For now, we'll use a simple heuristic
            if (IsLikelyHoliday(date))
            {
                multiplier *= cache.HolidayMultiplier;
            }

            // Peak season check (simple heuristic)
            if (IsPeakSeason(date))
            {
                multiplier *= cache.PeakSeasonMultiplier;
            }

            return multiplier;
        }

        private bool IsLikelyHoliday(DateTime date)
        {
            // Simple holiday heuristic - major holidays
            return (date.Month == 12 && date.Day >= 24 && date.Day <= 31) || // Christmas/New Year
                   (date.Month == 7 && date.Day == 4) || // July 4th
                   (date.Month == 11 && date.Day >= 22 && date.Day <= 28); // Thanksgiving week
        }

        private bool IsPeakSeason(DateTime date)
        {
            // Simple peak season heuristic - summer and winter holidays
            return (date.Month >= 6 && date.Month <= 8) || // Summer
                   (date.Month == 12) || // December
                   (date.Month >= 3 && date.Month <= 4); // Spring break
        }

        #endregion
    }
}