using System;
using System.Threading.Tasks;

namespace VisitaBookingApi.Services.Interfaces
{
    public interface IDistributedLockService
    {
        /// <summary>
        /// Acquires a distributed lock for the specified key
        /// </summary>
        /// <param name="key">Lock key</param>
        /// <param name="expiry">Lock expiry time</param>
        /// <returns>Lock token if successful, null if failed</returns>
        Task<string?> AcquireLockAsync(string key, TimeSpan expiry);

        /// <summary>
        /// Acquires multiple distributed locks atomically for a date range
        /// </summary>
        /// <param name="roomId">Room ID</param>
        /// <param name="checkInDate">Check-in date</param>
        /// <param name="checkOutDate">Check-out date</param>
        /// <param name="expiry">Lock expiry time</param>
        /// <returns>Lock token if all dates locked successfully, null if any date fails</returns>
        Task<string?> AcquireBookingLockAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, TimeSpan expiry);

        /// <summary>
        /// Releases a distributed lock
        /// </summary>
        /// <param name="key">Lock key</param>
        /// <param name="token">Lock token received from AcquireLockAsync</param>
        /// <returns>True if released successfully</returns>
        Task<bool> ReleaseLockAsync(string key, string token);

        /// <summary>
        /// Releases multiple distributed locks for a booking
        /// </summary>
        /// <param name="roomId">Room ID</param>
        /// <param name="checkInDate">Check-in date</param>
        /// <param name="checkOutDate">Check-out date</param>
        /// <param name="token">Lock token received from AcquireBookingLockAsync</param>
        /// <returns>True if all locks released successfully</returns>
        Task<bool> ReleaseBookingLockAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, string token);

        /// <summary>
        /// Extends the expiry of an existing lock
        /// </summary>
        /// <param name="key">Lock key</param>
        /// <param name="token">Lock token</param>
        /// <param name="expiry">New expiry time</param>
        /// <returns>True if extended successfully</returns>
        Task<bool> ExtendLockAsync(string key, string token, TimeSpan expiry);
    }
}