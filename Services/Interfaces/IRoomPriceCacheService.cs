using visita_booking_api.Models.Entities;
using visita_booking_api.Models.DTOs;

namespace visita_booking_api.Services.Interfaces
{
    public interface IRoomPriceCacheService
    {
        /// <summary>
        /// Update price cache for a specific room when pricing rules change
        /// </summary>
        Task UpdateRoomPriceCacheAsync(int roomId);
        
        /// <summary>
        /// Update price cache for multiple rooms (bulk operation)
        /// </summary>
        Task UpdateRoomsPriceCacheAsync(List<int> roomIds);
        
        /// <summary>
        /// Get cached price range for a room for quick filtering
        /// </summary>
        Task<RoomPriceCache?> GetRoomPriceCacheAsync(int roomId);
        
        /// <summary>
        /// Filter room IDs by price range using cached data (fast pre-filter)
        /// </summary>
        Task<List<int>> FilterRoomIdsByPriceRangeAsync(
            decimal? minPrice, 
            decimal? maxPrice, 
            DateTime checkIn, 
            DateTime checkOut, 
            List<int>? roomIds = null);

        /// <summary>
        /// Get room IDs that should be EXCLUDED from search results based on price range
        /// This is more efficient when most rooms match the criteria (negative filtering)
        /// </summary>
        Task<List<int>> GetRoomIdsToExcludeByPriceRangeAsync(
            decimal? minPrice,
            decimal? maxPrice,
            DateTime checkIn,
            DateTime checkOut,
            List<int>? roomIds = null);
        
        /// <summary>
        /// Calculate estimated price range for a date range using cache + multipliers
        /// </summary>
        Task<(decimal EstimatedMin, decimal EstimatedMax)> EstimatePriceRangeAsync(
            int roomId, 
            DateTime checkIn, 
            DateTime checkOut);
        
        /// <summary>
        /// Invalidate cache for rooms affected by pricing rule changes
        /// </summary>
        Task InvalidatePriceCacheAsync(List<int> roomIds);
        
        /// <summary>
        /// Background job to refresh stale price caches
        /// </summary>
        Task RefreshStalePriceCachesAsync(int batchSize = 100);
        
        /// <summary>
        /// Rebuild all price caches (admin operation)
        /// </summary>
        Task RebuildAllPriceCachesAsync();
        
        /// <summary>
        /// Get cache statistics for monitoring
        /// </summary>
        Task<PriceCacheStatsDTO> GetCacheStatsAsync();
    }
}