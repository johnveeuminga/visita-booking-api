using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VisitaBookingApi.Services.Interfaces;

namespace VisitaBookingApi.Services.Implementation
{
    public class SimpleDistributedLockService : IDistributedLockService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<SimpleDistributedLockService> _logger;

        public SimpleDistributedLockService(IDistributedCache cache, ILogger<SimpleDistributedLockService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<string?> AcquireLockAsync(string key, TimeSpan expiry)
        {
            try
            {
                var token = Guid.NewGuid().ToString("N");
                var lockKey = $"lock:{key}";

                // Check if lock already exists
                var existingLock = await _cache.GetStringAsync(lockKey);
                if (existingLock != null)
                {
                    _logger.LogDebug("Failed to acquire lock '{LockKey}' - already exists", lockKey);
                    return null;
                }

                // Try to set the lock
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry
                };

                await _cache.SetStringAsync(lockKey, token, options);
                
                // Verify we actually got the lock (basic race condition check)
                var verification = await _cache.GetStringAsync(lockKey);
                if (verification == token)
                {
                    _logger.LogDebug("Successfully acquired lock '{LockKey}' with token '{Token}'", lockKey, token);
                    return token;
                }

                _logger.LogDebug("Failed to acquire lock '{LockKey}' - lost race condition", lockKey);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring lock for key: {Key}", key);
                return null;
            }
        }

        public async Task<bool> ReleaseLockAsync(string key, string token)
        {
            try
            {
                var lockKey = $"lock:{key}";
                
                // Check if the lock exists and has the correct token
                var existingToken = await _cache.GetStringAsync(lockKey);
                if (existingToken != token)
                {
                    _logger.LogWarning("Cannot release lock '{LockKey}' - token mismatch or lock doesn't exist", lockKey);
                    return false;
                }

                // Remove the lock
                await _cache.RemoveAsync(lockKey);
                _logger.LogDebug("Successfully released lock '{LockKey}' with token '{Token}'", lockKey, token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing lock for key: {Key}", key);
                return false;
            }
        }

        public async Task<string?> AcquireBookingLockAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, TimeSpan expiry)
        {
            try
            {
                var token = Guid.NewGuid().ToString("N");
                var currentDate = checkInDate.Date;
                var lockedDates = new List<string>();

                // Try to acquire locks for each date in the range
                while (currentDate < checkOutDate.Date)
                {
                    var dateKey = $"booking:room:{roomId}:date:{currentDate:yyyy-MM-dd}";
                    var lockKey = $"lock:{dateKey}";

                    // Check if lock already exists
                    var existingLock = await _cache.GetStringAsync(lockKey);
                    if (existingLock != null)
                    {
                        _logger.LogDebug("Failed to acquire booking lock for room {RoomId} on {Date} - already exists", roomId, currentDate);
                        
                        // Release any locks we already acquired
                        await ReleaseBookingLocksAsync(lockedDates, token);
                        return null;
                    }

                    // Set the lock
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiry
                    };

                    await _cache.SetStringAsync(lockKey, token, options);
                    lockedDates.Add(lockKey);

                    currentDate = currentDate.AddDays(1);
                }

                _logger.LogDebug("Successfully acquired booking locks for room {RoomId} from {CheckIn} to {CheckOut}", roomId, checkInDate, checkOutDate);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring booking lock for room {RoomId}", roomId);
                return null;
            }
        }

        public async Task<bool> ReleaseBookingLockAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, string token)
        {
            try
            {
                var currentDate = checkInDate.Date;
                var success = true;

                while (currentDate < checkOutDate.Date)
                {
                    var dateKey = $"booking:room:{roomId}:date:{currentDate:yyyy-MM-dd}";
                    var lockKey = $"lock:{dateKey}";

                    // Check if the lock exists and has the correct token
                    var existingToken = await _cache.GetStringAsync(lockKey);
                    if (existingToken == token)
                    {
                        await _cache.RemoveAsync(lockKey);
                    }
                    else if (existingToken != null)
                    {
                        _logger.LogWarning("Cannot release booking lock for room {RoomId} on {Date} - token mismatch", roomId, currentDate);
                        success = false;
                    }

                    currentDate = currentDate.AddDays(1);
                }

                if (success)
                {
                    _logger.LogDebug("Successfully released booking locks for room {RoomId} from {CheckIn} to {CheckOut}", roomId, checkInDate, checkOutDate);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing booking lock for room {RoomId}", roomId);
                return false;
            }
        }

        private async Task ReleaseBookingLocksAsync(List<string> lockKeys, string token)
        {
            foreach (var lockKey in lockKeys)
            {
                try
                {
                    var existingToken = await _cache.GetStringAsync(lockKey);
                    if (existingToken == token)
                    {
                        await _cache.RemoveAsync(lockKey);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error releasing booking lock {LockKey}", lockKey);
                }
            }
        }

        public async Task<bool> ExtendLockAsync(string key, string token, TimeSpan expiry)
        {
            try
            {
                var lockKey = $"lock:{key}";
                
                // Check if the lock exists and has the correct token
                var existingToken = await _cache.GetStringAsync(lockKey);
                if (existingToken != token)
                {
                    _logger.LogWarning("Cannot extend lock '{LockKey}' - token mismatch or lock doesn't exist", lockKey);
                    return false;
                }

                // Extend the lock by setting it again with new expiry
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry
                };

                await _cache.SetStringAsync(lockKey, token, options);
                _logger.LogDebug("Successfully extended lock '{LockKey}' with token '{Token}'", lockKey, token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending lock for key: {Key}", key);
                return false;
            }
        }

        public async Task<bool> IsLockHeldAsync(string key, string token)
        {
            try
            {
                var lockKey = $"lock:{key}";
                var existingToken = await _cache.GetStringAsync(lockKey);
                return existingToken == token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking lock status for key: {Key}", key);
                return false;
            }
        }

        public async Task<bool> AcquireLockWithRetryAsync(string key, TimeSpan expiry, int maxRetries, TimeSpan retryDelay)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                var token = await AcquireLockAsync(key, expiry);
                if (token != null)
                {
                    return true;
                }

                if (i < maxRetries - 1) // Don't delay after the last attempt
                {
                    await Task.Delay(retryDelay);
                }
            }

            _logger.LogWarning("Failed to acquire lock '{Key}' after {MaxRetries} attempts", key, maxRetries);
            return false;
        }

        public async Task ForceReleaseLockAsync(string key)
        {
            try
            {
                var lockKey = $"lock:{key}";
                await _cache.RemoveAsync(lockKey);
                _logger.LogInformation("Force released lock '{LockKey}'", lockKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error force releasing lock for key: {Key}", key);
            }
        }
    }
}