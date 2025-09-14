using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisitaBookingApi.Services.Interfaces;

namespace VisitaBookingApi.Services.Implementation
{
    public class RedisDistributedLockService : IDistributedLockService
    {
        private readonly IDatabase _redis;
        private readonly ILogger<RedisDistributedLockService> _logger;

        public RedisDistributedLockService(IConnectionMultiplexer redis, ILogger<RedisDistributedLockService> logger)
        {
            _redis = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<string?> AcquireLockAsync(string key, TimeSpan expiry)
        {
            try
            {
                var token = Guid.NewGuid().ToString("N");
                var lockKey = $"lock:{key}";

                // Use SET with NX (only if not exists) and EX (expiry)
                var acquired = await _redis.StringSetAsync(lockKey, token, expiry, When.NotExists);
                
                if (acquired)
                {
                    _logger.LogDebug("Acquired distributed lock for key: {Key} with token: {Token}", key, token);
                    return token;
                }

                _logger.LogDebug("Failed to acquire distributed lock for key: {Key}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring distributed lock for key: {Key}", key);
                return null;
            }
        }

        public async Task<string?> AcquireBookingLockAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, TimeSpan expiry)
        {
            try
            {
                var token = Guid.NewGuid().ToString("N");
                var lockKeys = GenerateDateLockKeys(roomId, checkInDate, checkOutDate);

                _logger.LogDebug("Attempting to acquire booking locks for room {RoomId} from {CheckIn} to {CheckOut}. Keys: {Keys}", 
                    roomId, checkInDate.ToString("yyyy-MM-dd"), checkOutDate.ToString("yyyy-MM-dd"), string.Join(", ", lockKeys));

                // Use Lua script to atomically acquire all date locks or none
                var script = @"
                    local token = ARGV[1]
                    local expiry = ARGV[2]
                    
                    -- Check if any date locks already exist
                    for i = 1, #KEYS do
                        if redis.call('EXISTS', KEYS[i]) == 1 then
                            return nil  -- At least one date is already locked
                        end
                    end
                    
                    -- Acquire all date locks
                    for i = 1, #KEYS do
                        redis.call('SETEX', KEYS[i], expiry, token)
                    end
                    
                    return token";

                var redisKeys = lockKeys.Select(k => (RedisKey)k).ToArray();
                var result = await _redis.ScriptEvaluateAsync(script, 
                    redisKeys, 
                    new RedisValue[] { token, (int)expiry.TotalSeconds });

                if (result.IsNull)
                {
                    _logger.LogDebug("Failed to acquire booking locks for room {RoomId} - some dates already locked", roomId);
                    return null;
                }

                _logger.LogDebug("Successfully acquired booking locks for room {RoomId} with token: {Token}", roomId, token);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring booking locks for room {RoomId}", roomId);
                return null;
            }
        }

        public async Task<bool> ReleaseLockAsync(string key, string token)
        {
            try
            {
                var lockKey = $"lock:{key}";
                
                // Lua script to ensure we only delete if we own the lock
                const string script = @"
                    if redis.call('GET', KEYS[1]) == ARGV[1] then
                        return redis.call('DEL', KEYS[1])
                    else
                        return 0
                    end";

                var result = await _redis.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { token });
                var released = result.ToString() == "1";

                if (released)
                {
                    _logger.LogDebug("Released distributed lock for key: {Key}", key);
                }
                else
                {
                    _logger.LogWarning("Failed to release distributed lock for key: {Key} - token mismatch or expired", key);
                }

                return released;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing distributed lock for key: {Key}", key);
                return false;
            }
        }

        public async Task<bool> ReleaseBookingLockAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, string token)
        {
            try
            {
                var lockKeys = GenerateDateLockKeys(roomId, checkInDate, checkOutDate);

                _logger.LogDebug("Attempting to release booking locks for room {RoomId}. Keys: {Keys}", 
                    roomId, string.Join(", ", lockKeys));

                // Use Lua script to atomically release all date locks owned by this token
                var script = @"
                    local token = ARGV[1]
                    local released = 0
                    
                    for i = 1, #KEYS do
                        if redis.call('GET', KEYS[i]) == token then
                            redis.call('DEL', KEYS[i])
                            released = released + 1
                        end
                    end
                    
                    return released";

                var redisKeys = lockKeys.Select(k => (RedisKey)k).ToArray();
                var result = await _redis.ScriptEvaluateAsync(script, redisKeys, new RedisValue[] { token });
                
                var releasedCount = (int)result;
                var success = releasedCount > 0;

                if (success)
                {
                    _logger.LogDebug("Released {Count} booking locks for room {RoomId}", releasedCount, roomId);
                }
                else
                {
                    _logger.LogWarning("Failed to release booking locks for room {RoomId} - token mismatch or already expired", roomId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing booking locks for room {RoomId}", roomId);
                return false;
            }
        }

        public async Task<bool> ExtendLockAsync(string key, string token, TimeSpan expiry)
        {
            try
            {
                var lockKey = $"lock:{key}";
                
                // Lua script to extend expiry only if we own the lock
                const string script = @"
                    if redis.call('GET', KEYS[1]) == ARGV[1] then
                        return redis.call('EXPIRE', KEYS[1], ARGV[2])
                    else
                        return 0
                    end";

                var result = await _redis.ScriptEvaluateAsync(script, 
                    new RedisKey[] { lockKey }, 
                    new RedisValue[] { token, (int)expiry.TotalSeconds });

                var extended = result.ToString() == "1";

                if (extended)
                {
                    _logger.LogDebug("Extended distributed lock for key: {Key}", key);
                }

                return extended;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending distributed lock for key: {Key}", key);
                return false;
            }
        }

        private List<string> GenerateDateLockKeys(int roomId, DateTime checkInDate, DateTime checkOutDate)
        {
            var keys = new List<string>();
            
            // Generate a lock key for each date in the booking range
            // Note: We lock check-in date up to (but not including) check-out date
            for (var date = checkInDate.Date; date < checkOutDate.Date; date = date.AddDays(1))
            {
                keys.Add($"lock:booking:room:{roomId}:date:{date:yyyyMMdd}");
            }
            
            return keys;
        }

    }
}