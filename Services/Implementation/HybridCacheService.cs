using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Services.Implementation
{
    public class HybridCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer? _redis;
        private readonly ILogger<HybridCacheService> _logger;
        private readonly IConfiguration _configuration;
        
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false
        };

        public HybridCacheService(
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            IConnectionMultiplexer? redis,
            ILogger<HybridCacheService> logger,
            IConfiguration configuration)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _redis = redis;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var keyPrefix = _configuration["Caching:Redis:KeyPrefix"] ?? "visita:";
                var fullKey = $"{keyPrefix}{key}";

                // Try memory cache first (fastest)
                if (_memoryCache.TryGetValue(fullKey, out T? cachedValue) && cachedValue != null)
                {
                    _logger.LogDebug("Cache hit from memory cache for key: {Key}", key);
                    return cachedValue;
                }

                // Try distributed cache (Redis)
                var distributedValue = await _distributedCache.GetStringAsync(fullKey);
                if (!string.IsNullOrEmpty(distributedValue))
                {
                    var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue, _jsonOptions);
                    if (deserializedValue != null)
                    {
                        // Store in memory cache for faster access next time
                        var memoryTtl = TimeSpan.FromMinutes(5); // Shorter TTL for memory cache
                        var cacheEntryOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = memoryTtl,
                            Size = 1
                        };
                        _memoryCache.Set(fullKey, deserializedValue, cacheEntryOptions);
                        
                        _logger.LogDebug("Cache hit from Redis for key: {Key}", key);
                        return deserializedValue;
                    }
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
        {
            try
            {
                var keyPrefix = _configuration["Caching:Redis:KeyPrefix"] ?? "visita:";
                var fullKey = $"{keyPrefix}{key}";

                if (value == null) return;

                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);

                // Set in both memory and distributed cache
                var memoryTtl = expiration > TimeSpan.FromMinutes(10) ? TimeSpan.FromMinutes(10) : expiration;
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = memoryTtl,
                    Size = 1 // Set a size for the cache entry
                };
                _memoryCache.Set(fullKey, value, cacheEntryOptions);

                var distributedOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };
                await _distributedCache.SetStringAsync(fullKey, serializedValue, distributedOptions);

                _logger.LogDebug("Cached value for key: {Key} with TTL: {TTL}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
        }

        public async Task SetAsync<T>(string key, T value, int expirationSeconds) where T : class
        {
            await SetAsync(key, value, TimeSpan.FromSeconds(expirationSeconds));
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                var keyPrefix = _configuration["Caching:Redis:KeyPrefix"] ?? "visita:";
                var fullKey = $"{keyPrefix}{key}";

                // Remove from both caches
                _memoryCache.Remove(fullKey);
                await _distributedCache.RemoveAsync(fullKey);

                _logger.LogDebug("Removed cache value for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var keyPrefix = _configuration["Caching:Redis:KeyPrefix"] ?? "visita:";
                var fullPattern = $"{keyPrefix}{pattern}";

                if (_redis != null)
                {
                    var database = _redis.GetDatabase();
                    var server = _redis.GetServer(_redis.GetEndPoints().First());
                    
                    var keys = server.Keys(pattern: fullPattern);
                    if (keys.Any())
                    {
                        await database.KeyDeleteAsync(keys.ToArray());
                        
                        // Also remove from memory cache
                        foreach (var key in keys)
                        {
                            _memoryCache.Remove(key.ToString());
                        }
                        
                        _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keys.Count(), pattern);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache values by pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var keyPrefix = _configuration["Caching:Redis:KeyPrefix"] ?? "visita:";
                var fullKey = $"{keyPrefix}{key}";

                // Check memory cache first
                if (_memoryCache.TryGetValue(fullKey, out _))
                    return true;

                // Check Redis
                if (_redis != null)
                {
                    var database = _redis.GetDatabase();
                    return await database.KeyExistsAsync(fullKey);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public async Task SetStringAsync(string key, string value, TimeSpan expiration)
        {
            try
            {
                var keyPrefix = _configuration["Caching:Redis:KeyPrefix"] ?? "visita:";
                var fullKey = $"{keyPrefix}{key}";

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };
                await _distributedCache.SetStringAsync(fullKey, value, options);

                // Also store in memory with shorter TTL
                var memoryTtl = expiration > TimeSpan.FromMinutes(10) ? TimeSpan.FromMinutes(10) : expiration;
                var memoryCacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = memoryTtl,
                    Size = 1
                };
                _memoryCache.Set(fullKey, value, memoryCacheOptions);

                _logger.LogDebug("Cached string value for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting string cache value for key: {Key}", key);
            }
        }

        public async Task<string?> GetStringAsync(string key)
        {
            try
            {
                var keyPrefix = _configuration["Caching:Redis:KeyPrefix"] ?? "visita:";
                var fullKey = $"{keyPrefix}{key}";

                // Try memory cache first
                if (_memoryCache.TryGetValue(fullKey, out string? memoryValue))
                    return memoryValue;

                // Try distributed cache
                var distributedValue = await _distributedCache.GetStringAsync(fullKey);
                if (!string.IsNullOrEmpty(distributedValue))
                {
                    // Store in memory cache
                    var memoryCacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                        Size = 1
                    };
                    _memoryCache.Set(fullKey, distributedValue, memoryCacheOptions);
                }

                return distributedValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting string cache value for key: {Key}", key);
                return null;
            }
        }
    }
}