using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace visita_booking_api.Services.Interfaces
{
    public interface IAvailabilityLedgerService
    {
        /// <summary>
        /// Generate availability ledger for the given date range (inclusive start, exclusive end) and return number of rooms processed.
        /// This runs the SQL aggregation against the DB and writes per-room ledger data to Redis.
        /// </summary>
        Task<int> GenerateLedgerAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Warm up ledger for a single room and date range (writes ledger for that room only).
        /// </summary>
    Task<bool> WarmupRoomLedgerAsync(int roomId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Try to read min available units for the given rooms across the date range from the ledger (cache).
    /// Returns a dictionary roomId -> minAvailable; rooms with no ledger data will be absent.
    /// </summary>
    Task<Dictionary<int,int>> TryGetMinAvailableFromLedgerAsync(List<int> roomIds, DateTime startDate, DateTime endDate);
    }
}
