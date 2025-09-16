using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VisitaBookingApi.Data;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Services.Implementation
{
    public class AvailabilityLedgerService : IAvailabilityLedgerService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICacheService _cacheService;
        private readonly IConnectionMultiplexer? _redis;
        private readonly ILogger<AvailabilityLedgerService> _logger;
        private readonly IConfiguration _configuration;

        public AvailabilityLedgerService(
            ApplicationDbContext db,
            ICacheService cacheService,
            ILogger<AvailabilityLedgerService> logger,
            IConfiguration configuration)
        {
            _db = db;
            _cacheService = cacheService;
            _logger = logger;
            _configuration = configuration;
            // We don't require an IConnectionMultiplexer; the service will fall back to ICacheService when Redis isn't available.
            _redis = null;
        }

        public async Task<int> GenerateLedgerAsync(DateTime startDate, DateTime endDate)
        {
            // Basic SQL to generate per-room per-date available units using existing tables.
            // The SQL intentionally keeps logic simple and leverages EF Core connection for portability.

            // We'll aggregate ledger data in C# by querying overrides, bookings, reservations and locks.

            // We will not run the above raw SQL directly because of portability; instead we will generate ledger in C# by
            // fetching rooms, overrides and aggregated consumption from DB then writing to Redis.

            var rooms = await _db.Rooms.AsNoTracking().ToListAsync();

            // Fetch overrides in range
            var overrides = await _db.RoomAvailabilityOverrides
                .AsNoTracking()
                .Where(o => o.Date >= startDate && o.Date < endDate)
                .ToListAsync();

            // Fetch bookings/reservations/locks consumption per room per date
            var bookings = await _db.Bookings
                .AsNoTracking()
                .Where(b => b.CheckInDate < endDate && b.CheckOutDate > startDate && b.Status != BookingStatus.Cancelled)
                .Select(b => new { b.RoomId, b.CheckInDate, b.CheckOutDate, b.Quantity })
                .ToListAsync();

            var reservations = await _db.BookingReservations
                .AsNoTracking()
                .Where(r => r.CheckInDate < endDate && r.CheckOutDate > startDate && r.Status != ReservationStatus.Expired)
                .Select(r => new { r.RoomId, r.CheckInDate, r.CheckOutDate, r.Quantity })
                .ToListAsync();

            var locks = await _db.BookingAvailabilityLocks
                .AsNoTracking()
                .Where(l => l.CheckInDate < endDate && l.CheckOutDate > startDate && l.IsActive)
                .Select(l => new { l.RoomId, l.CheckInDate, l.CheckOutDate, l.Quantity })
                .ToListAsync();

            var totalProcessedRooms = 0;
            var keyPrefix = _configuration["Caching:Redis:KeyPrefix"] ?? "visita:";

            // Build a date list
            var dates = new List<DateTime>();
            for (var dt = startDate.Date; dt < endDate.Date; dt = dt.AddDays(1))
                dates.Add(dt);

            foreach (var room in rooms)
            {
                var ledger = new Dictionary<string, string>();

                foreach (var date in dates)
                {
                    var overrideRow = overrides.FirstOrDefault(o => o.RoomId == room.Id && o.Date.Date == date);
                    var baseUnits = overrideRow?.AvailableCount ?? room.TotalUnits;

                    // Sum consumed from bookings/reservations/locks for this room on this date
                    int consumed = 0;

                    consumed += bookings.Where(b => b.RoomId == room.Id && date >= b.CheckInDate && date < b.CheckOutDate).Sum(b => b.Quantity);
                    consumed += reservations.Where(r => r.RoomId == room.Id && date >= r.CheckInDate && date < r.CheckOutDate).Sum(r => r.Quantity);
                    consumed += locks.Where(l => l.RoomId == room.Id && date >= l.CheckInDate && date < l.CheckOutDate).Sum(l => l.Quantity);

                    var available = baseUnits - consumed;
                    if (available < 0) available = 0;

                    ledger[date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)] = available.ToString();
                }

                // Write ledger to Redis as a hash: key = {prefix}room:ledger:{roomId}
                var redisKey = $"{keyPrefix}room:ledger:{room.Id}";

                if (_redis != null)
                {
                    var db = _redis.GetDatabase();
                    var entries = ledger.Select(kv => new HashEntry(kv.Key, kv.Value)).ToArray();
                    await db.HashSetAsync(redisKey, entries);
                }
                else
                {
                    // Fallback: store JSON string via ICacheService
                    var json = JsonSerializer.Serialize(ledger);
                    await _cacheService.SetStringAsync(redisKey, json, TimeSpan.FromDays(30));
                }

                totalProcessedRooms++;
            }

            _logger.LogInformation("Availability ledger generated for {Count} rooms for range {Start} - {End}", totalProcessedRooms, startDate, endDate);
            return totalProcessedRooms;
        }

        public async Task<bool> WarmupRoomLedgerAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            var rooms = await _db.Rooms.AsNoTracking().Where(r => r.Id == roomId).ToListAsync();
            if (!rooms.Any()) return false;

            var overrides = await _db.RoomAvailabilityOverrides
                .AsNoTracking()
                .Where(o => o.RoomId == roomId && o.Date >= startDate && o.Date < endDate)
                .ToListAsync();

            var bookings = await _db.Bookings
                .AsNoTracking()
                .Where(b => b.RoomId == roomId && b.CheckInDate < endDate && b.CheckOutDate > startDate && b.Status != BookingStatus.Cancelled)
                .Select(b => new { b.CheckInDate, b.CheckOutDate, b.Quantity })
                .ToListAsync();

            var reservations = await _db.BookingReservations
                .AsNoTracking()
                .Where(r => r.RoomId == roomId && r.CheckInDate < endDate && r.CheckOutDate > startDate && r.Status != ReservationStatus.Expired)
                .Select(r => new { r.CheckInDate, r.CheckOutDate, r.Quantity })
                .ToListAsync();

            var locks = await _db.BookingAvailabilityLocks
                .AsNoTracking()
                .Where(l => l.RoomId == roomId && l.CheckInDate < endDate && l.CheckOutDate > startDate && l.IsActive)
                .Select(l => new { l.CheckInDate, l.CheckOutDate, l.Quantity })
                .ToListAsync();

            var date = startDate.Date;
            var ledger = new Dictionary<string, string>();
            for (; date < endDate.Date; date = date.AddDays(1))
            {
                var overrideRow = overrides.FirstOrDefault(o => o.RoomId == roomId && o.Date.Date == date);
                var baseUnits = overrideRow?.AvailableCount ?? rooms.First().TotalUnits;

                int consumed = 0;
                consumed += bookings.Where(b => date >= b.CheckInDate && date < b.CheckOutDate).Sum(b => b.Quantity);
                consumed += reservations.Where(r => date >= r.CheckInDate && date < r.CheckOutDate).Sum(r => r.Quantity);
                consumed += locks.Where(l => date >= l.CheckInDate && date < l.CheckOutDate).Sum(l => l.Quantity);

                var available = baseUnits - consumed;
                if (available < 0) available = 0;
                ledger[date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)] = available.ToString();
            }

            var keyPrefix = _configuration["Caching:Redis:KeyPrefix"] ?? "visita:";
            var redisKey = $"{keyPrefix}room:ledger:{roomId}";

            if (_redis != null)
            {
                var db = _redis.GetDatabase();
                var entries = ledger.Select(kv => new HashEntry(kv.Key, kv.Value)).ToArray();
                await db.HashSetAsync(redisKey, entries);
            }
            else
            {
                var json = JsonSerializer.Serialize(ledger);
                await _cacheService.SetStringAsync(redisKey, json, TimeSpan.FromDays(30));
            }

            _logger.LogInformation("Warmed up ledger for room {RoomId} for range {Start} - {End}", roomId, startDate, endDate);
            return true;
        }

        public async Task<Dictionary<int,int>> TryGetMinAvailableFromLedgerAsync(List<int> roomIds, DateTime startDate, DateTime endDate)
        {
            var result = new Dictionary<int,int>();
            if (roomIds == null || !roomIds.Any()) return result;

            var keyPrefix = _configuration["Caching:Redis:KeyPrefix"] ?? "visita:";
            var dateKeys = new List<string>();
            for (var dt = startDate.Date; dt < endDate.Date; dt = dt.AddDays(1))
                dateKeys.Add(dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            if (_redis == null)
            {
                // Fallback: try to read JSON string from cacheService for each room
                foreach (var roomId in roomIds)
                {
                    var redisKey = $"{keyPrefix}room:ledger:{roomId}";
                    var json = await _cacheService.GetStringAsync(redisKey);
                    if (string.IsNullOrEmpty(json)) continue;

                    try
                    {
                        var dict = JsonSerializer.Deserialize<Dictionary<string,string>>(json) ?? new Dictionary<string,string>();
                        var values = dateKeys.Where(dk => dict.ContainsKey(dk)).Select(dk => int.TryParse(dict[dk], out var v) ? v : 0).ToList();
                        if (!values.Any()) continue;
                        result[roomId] = values.Min();
                    }
                    catch { continue; }
                }

                return result;
            }

            var db = _redis.GetDatabase();
            foreach (var roomId in roomIds)
            {
                var redisKey = $"{keyPrefix}room:ledger:{roomId}";
                // HMGET multiple fields
                var redisFields = dateKeys.Select(k => (RedisValue)k).ToArray();
                var values = await db.HashGetAsync(redisKey, redisFields);
                if (values == null || values.Length == 0) continue;

                var intVals = values.Where(v => v.HasValue).Select(v => {
                    if (int.TryParse(v, out var x)) return x;
                    return 0;
                }).ToList();

                if (!intVals.Any()) continue;
                result[roomId] = intVals.Min();
            }

            return result;
        }
    }
}
