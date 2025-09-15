using System;

namespace visita_booking_api.Services
{
    public interface ITimezoneService
    {
        /// <summary>
        /// Gets the current date and time in GMT+8 timezone
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// Gets the current date (without time) in GMT+8 timezone
        /// </summary>
        DateTime Today { get; }

        /// <summary>
        /// Converts UTC datetime to GMT+8
        /// </summary>
        DateTime ConvertToLocalTime(DateTime utcDateTime);

        /// <summary>
        /// Converts GMT+8 datetime to UTC
        /// </summary>
        DateTime ConvertToUtc(DateTime localDateTime);

        /// <summary>
        /// Gets the GMT+8 timezone info
        /// </summary>
        TimeZoneInfo LocalTimeZone { get; }
    }
}