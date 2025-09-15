using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Services.Implementation
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _keyPrefix;
        
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false
        };

        public RedisCacheService(
            IDistributedCache distributedCache,
            ILogger<RedisCacheService> logger,
            IConfiguration configuration)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _configuration = configuration;
            _keyPrefix = _configuration["Caching:Redis:KeyPrefix"] ?? "visita:";
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var fullKey = $"{_keyPrefix}{key}";
                var cachedValue = await _distributedCache.GetStringAsync(fullKey);
                
                if (cachedValue == null)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return null;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
        {
            try
            {
                var fullKey = $"{_keyPrefix}{key}";
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };
                
                await _distributedCache.SetStringAsync(fullKey, json, options);
                _logger.LogDebug("Cached value for key: {Key}, expires in: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching value for key: {Key}", key);
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
                var fullKey = $"{_keyPrefix}{key}";
                await _distributedCache.RemoveAsync(fullKey);
                _logger.LogDebug("Removed cached value for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                // Note: Pattern-based deletion is not supported with IDistributedCache for Upstash Redis HTTP REST
                // This is a limitation when using only IDistributedCache
                _logger.LogWarning("Pattern-based cache removal is not supported with IDistributedCache for Upstash Redis. Pattern: {Pattern}", pattern);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cached values by pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var fullKey = $"{_keyPrefix}{key}";
                var value = await _distributedCache.GetStringAsync(fullKey);
                return value != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key exists: {Key}", key);
                return false;
            }
        }

        public async Task SetStringAsync(string key, string value, TimeSpan expiration)
        {
            try
            {
                var fullKey = $"{_keyPrefix}{key}";
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };
                
                await _distributedCache.SetStringAsync(fullKey, value, options);
                _logger.LogDebug("Cached string value for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching string value for key: {Key}", key);
            }
        }

        public async Task<string?> GetStringAsync(string key)
        {
            try
            {
                var fullKey = $"{_keyPrefix}{key}";
                return await _distributedCache.GetStringAsync(fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached string for key: {Key}", key);
                return null;
            }
        }
    }
}