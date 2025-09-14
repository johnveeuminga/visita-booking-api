using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using VisitaBookingApi.Services.Implementation;
using VisitaBookingApi.Services.Interfaces;

namespace VisitaBookingApi.Tests
{
    /// <summary>
    /// Simple test to demonstrate distributed locking behavior
    /// This would typically be run as part of a unit test suite
    /// </summary>
    public class DistributedLockingDemo
    {
        private readonly IDistributedLockService _lockService;
        private readonly ILogger<DistributedLockingDemo> _logger;

        public DistributedLockingDemo(IDistributedLockService lockService, ILogger<DistributedLockingDemo> logger)
        {
            _lockService = lockService;
            _logger = logger;
        }

        /// <summary>
        /// Demonstrates how the multi-date distributed lock prevents race conditions for overlapping bookings
        /// </summary>
        public async Task DemonstrateOverlapPrevention()
        {
            var roomId = 101;
            
            // Scenario from user's example:
            // User A: Sep 14-18 (CheckIn: Sep 14, CheckOut: Sep 18)
            // User B: Sep 15-17 (CheckIn: Sep 15, CheckOut: Sep 17)
            // User B should be BLOCKED because dates overlap
            
            var userACheckIn = new DateTime(2025, 9, 14);
            var userACheckOut = new DateTime(2025, 9, 18);
            
            var userBCheckIn = new DateTime(2025, 9, 15);
            var userBCheckOut = new DateTime(2025, 9, 17);

            var task1 = SimulateOverlappingBookingAttempt(roomId, userACheckIn, userACheckOut, "User A", 1);
            var task2 = SimulateOverlappingBookingAttempt(roomId, userBCheckIn, userBCheckOut, "User B", 2);

            // Start both tasks simultaneously
            await Task.WhenAll(task1, task2);
        }

        private async Task SimulateOverlappingBookingAttempt(int roomId, DateTime checkIn, DateTime checkOut, string userName, int userId)
        {
            _logger.LogInformation("{UserName} attempting to book room {RoomId} from {CheckIn} to {CheckOut}", 
                userName, roomId, checkIn.ToString("MMM dd"), checkOut.ToString("MMM dd"));

            // Attempt to acquire booking lock for the date range
            var lockToken = await _lockService.AcquireBookingLockAsync(roomId, checkIn, checkOut, TimeSpan.FromMinutes(1));

            if (lockToken == null)
            {
                _logger.LogWarning("{UserName} BLOCKED - Another user has already booked overlapping dates!", userName);
                return;
            }

            _logger.LogInformation("{UserName} ACQUIRED locks for date range - proceeding with booking", userName);

            try
            {
                // Simulate booking process
                _logger.LogInformation("{UserName} is processing booking for {Days} days...", userName, (checkOut - checkIn).Days);
                await Task.Delay(2000); // Simulate 2 seconds of work

                _logger.LogInformation("{UserName} completed booking successfully!", userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{UserName} encountered error during booking", userName);
            }
            finally
            {
                // Always release all date locks
                var released = await _lockService.ReleaseBookingLockAsync(roomId, checkIn, checkOut, lockToken);
                _logger.LogInformation("{UserName} released all date locks: {Released}", userName, released);
            }
        }

        /// <summary>
        /// Test lock timeout behavior
        /// </summary>
        public async Task TestLockTimeout()
        {
            var lockKey = "booking:room:102:20240916:20240918";
            
            _logger.LogInformation("Testing lock timeout behavior...");

            // Acquire lock with very short expiry
            var lockToken = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(2));
            
            if (lockToken != null)
            {
                _logger.LogInformation("Lock acquired, waiting for timeout...");
                
                // Wait longer than the expiry time
                await Task.Delay(3000);
                
                // Try to extend the expired lock (should fail)
                var extended = await _lockService.ExtendLockAsync(lockKey, lockToken, TimeSpan.FromMinutes(1));
                _logger.LogInformation("Extend expired lock result: {Extended}", extended);
                
                // Try to release expired lock (should fail)
                var released = await _lockService.ReleaseLockAsync(lockKey, lockToken);
                _logger.LogInformation("Release expired lock result: {Released}", released);
                
                // Now try to acquire the same lock (should succeed since previous expired)
                var newToken = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromMinutes(1));
                _logger.LogInformation("New lock acquisition after timeout: {Success}", newToken != null);
                
                if (newToken != null)
                {
                    await _lockService.ReleaseLockAsync(lockKey, newToken);
                    _logger.LogInformation("New lock released successfully");
                }
            }
        }
    }

    /// <summary>
    /// Expected output when running DemonstrateOverlapPrevention():
    /// 
    /// User A attempting to book room 101 from Sep 14 to Sep 18
    /// User B attempting to book room 101 from Sep 15 to Sep 17
    /// User A ACQUIRED locks for date range - proceeding with booking
    /// User B BLOCKED - Another user has already booked overlapping dates!
    /// User A is processing booking for 4 days...
    /// User A completed booking successfully!
    /// User A released all date locks: True
    /// 
    /// This demonstrates that:
    /// 1. User A locks dates: Sep 14, 15, 16, 17
    /// 2. User B tries to lock dates: Sep 15, 16 
    /// 3. User B is blocked because Sep 15 is already locked by User A
    /// 4. No race condition occurs - overlapping bookings are prevented
    /// 5. All date locks are properly released after completion
    /// </summary>
}