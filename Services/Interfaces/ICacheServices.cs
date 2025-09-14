using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace visita_booking_api.Services.Interfaces
{
    public enum CacheInvalidationType
    {
        RoomUpdated,
        PricingUpdated,
        AvailabilityUpdated,
        BookingCreated,
        PhotosUpdated,
        AmenitiesUpdated,
        AmenityCreated,
        AmenityUpdated,
        AmenityDeleted
    }

    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;
        Task SetAsync<T>(string key, T value, int expirationSeconds) where T : class;
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        Task<bool> ExistsAsync(string key);
        Task SetStringAsync(string key, string value, TimeSpan expiration);
        Task<string?> GetStringAsync(string key);
    }

    public interface ICacheInvalidationService
    {
        Task InvalidateRoomCacheAsync(int roomId, CacheInvalidationType type);
        Task InvalidateSearchCacheAsync();
        Task InvalidateAvailabilityCacheAsync(int roomId);
        Task InvalidatePricingCacheAsync(int roomId);
        Task InvalidateAllRoomDataAsync(int roomId);
        
        // Amenity cache invalidation methods
        Task InvalidateAmenityCacheAsync(int amenityId);
        Task InvalidateAmenityListCacheAsync();
        Task InvalidateAmenityCategoriesCacheAsync();
    }

    public interface ICacheWarmupService
    {
        Task WarmupRoomCacheAsync(int roomId);
        Task WarmupPopularRoomsAsync();
        Task WarmupAmenitiesCacheAsync();
        Task WarmupAvailabilityCacheAsync(int roomId, DateTime startDate, DateTime endDate);
    }
}