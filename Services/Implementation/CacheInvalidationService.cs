using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.Cache;

namespace visita_booking_api.Services.Implementation
{
    public class CacheInvalidationService : ICacheInvalidationService
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheInvalidationService> _logger;

        public CacheInvalidationService(ICacheService cacheService, ILogger<CacheInvalidationService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task InvalidateRoomCacheAsync(int roomId, CacheInvalidationType type)
        {
            try
            {
                var tasks = new List<Task>();

                switch (type)
                {
                    case CacheInvalidationType.RoomUpdated:
                        tasks.Add(InvalidateRoomBasicDataAsync(roomId));
                        tasks.Add(InvalidateSearchCacheAsync());
                        break;

                    case CacheInvalidationType.PricingUpdated:
                        tasks.Add(InvalidatePricingCacheAsync(roomId));
                        tasks.Add(InvalidateSearchCacheAsync());
                        break;

                    case CacheInvalidationType.AvailabilityUpdated:
                    case CacheInvalidationType.BookingCreated:
                        tasks.Add(InvalidateAvailabilityCacheAsync(roomId));
                        tasks.Add(InvalidateSearchCacheAsync());
                        break;

                    case CacheInvalidationType.PhotosUpdated:
                        tasks.Add(_cacheService.RemoveAsync(CacheKeys.RoomPhotos(roomId)));
                        tasks.Add(_cacheService.RemoveAsync(CacheKeys.RoomDetails(roomId)));
                        break;

                    case CacheInvalidationType.AmenitiesUpdated:
                        tasks.Add(_cacheService.RemoveAsync(CacheKeys.RoomAmenities(roomId)));
                        tasks.Add(_cacheService.RemoveAsync(CacheKeys.RoomDetails(roomId)));
                        tasks.Add(_cacheService.RemoveAsync(CacheKeys.AMENITIES_ALL));
                        tasks.Add(InvalidateSearchCacheAsync());
                        break;
                        
                    case CacheInvalidationType.AmenityCreated:
                    case CacheInvalidationType.AmenityUpdated:
                    case CacheInvalidationType.AmenityDeleted:
                        tasks.Add(InvalidateAmenityListCacheAsync());
                        tasks.Add(InvalidateSearchCacheAsync());
                        break;
                }

                await Task.WhenAll(tasks);
                _logger.LogInformation("Cache invalidated for room {RoomId}, type: {Type}", roomId, type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache for room {RoomId}, type: {Type}", roomId, type);
            }
        }

        public async Task InvalidateSearchCacheAsync()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(CacheKeys.SearchPattern());
                _logger.LogDebug("Search cache invalidated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating search cache");
            }
        }

        public async Task InvalidateAvailabilityCacheAsync(int roomId)
        {
            try
            {
                var tasks = new List<Task>
                {
                    _cacheService.RemoveByPatternAsync(CacheKeys.AvailabilityPattern(roomId)),
                    _cacheService.RemoveByPatternAsync($"{CacheKeys.CALENDAR_MATRIX_PREFIX}{roomId}:*")
                };

                await Task.WhenAll(tasks);
                _logger.LogDebug("Availability cache invalidated for room {RoomId}", roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating availability cache for room {RoomId}", roomId);
            }
        }

        public async Task InvalidatePricingCacheAsync(int roomId)
        {
            try
            {
                await _cacheService.RemoveByPatternAsync($"{CacheKeys.ROOM_PRICING_PREFIX}{roomId}:*");
                _logger.LogDebug("Pricing cache invalidated for room {RoomId}", roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating pricing cache for room {RoomId}", roomId);
            }
        }

        public async Task InvalidateAllRoomDataAsync(int roomId)
        {
            try
            {
                var tasks = new List<Task>
                {
                    _cacheService.RemoveAsync(CacheKeys.RoomDetails(roomId)),
                    _cacheService.RemoveAsync(CacheKeys.RoomPhotos(roomId)),
                    _cacheService.RemoveAsync(CacheKeys.RoomAmenities(roomId)),
                    InvalidateAvailabilityCacheAsync(roomId),
                    InvalidatePricingCacheAsync(roomId),
                    InvalidateSearchCacheAsync()
                };

                await Task.WhenAll(tasks);
                _logger.LogInformation("All cache invalidated for room {RoomId}", roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating all cache for room {RoomId}", roomId);
            }
        }

        public async Task InvalidateAmenityCacheAsync(int amenityId)
        {
            try
            {
                var tasks = new List<Task>
                {
                    _cacheService.RemoveAsync($"amenity:details:{amenityId}"),
                    InvalidateAmenityListCacheAsync()
                };

                await Task.WhenAll(tasks);
                _logger.LogDebug("Amenity cache invalidated for amenity {AmenityId}", amenityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache for amenity {AmenityId}", amenityId);
            }
        }

        public async Task InvalidateAmenityListCacheAsync()
        {
            try
            {
                var tasks = new List<Task>
                {
                    _cacheService.RemoveAsync(CacheKeys.AMENITIES_ALL),
                    _cacheService.RemoveByPatternAsync("amenities:category:*"),
                    _cacheService.RemoveByPatternAsync("room:*:amenities")
                };

                await Task.WhenAll(tasks);
                _logger.LogDebug("Amenity list cache invalidated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating amenity list cache");
            }
        }

        public async Task InvalidateAmenityCategoriesCacheAsync()
        {
            try
            {
                await _cacheService.RemoveAsync("amenity:categories");
                _logger.LogDebug("Amenity categories cache invalidated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating amenity categories cache");
            }
        }

        private async Task InvalidateRoomBasicDataAsync(int roomId)
        {
            var tasks = new List<Task>
            {
                _cacheService.RemoveAsync(CacheKeys.RoomDetails(roomId)),
                _cacheService.RemoveAsync(CacheKeys.RoomPhotos(roomId)),
                _cacheService.RemoveAsync(CacheKeys.RoomAmenities(roomId))
            };

            await Task.WhenAll(tasks);
        }
    }
}