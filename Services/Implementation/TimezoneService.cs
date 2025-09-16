using System;

namespace visita_booking_api.Services.Implementation
{
    public class TimezoneService : ITimezoneService
    {
        private readonly TimeZoneInfo _localTimeZone;

        public TimezoneService()
        {
            // GMT+8 timezone (Philippine Standard Time / Singapore Standard Time)
            try
            {
                // Try to get Asia/Manila timezone (GMT+8)
                _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    // Fallback to Singapore timezone (also GMT+8)
                    _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                }
                catch (TimeZoneNotFoundException)
                {
                    // Final fallback - create custom GMT+8 timezone
                    _localTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                        "GMT+8",
                        TimeSpan.FromHours(8),
                        "GMT+8",
                        "GMT+8 Standard Time"
                    );
                }
            }
        }

        public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _localTimeZone);

        public DateTime Today => Now.Date;

        public DateTime ConvertToLocalTime(DateTime utcDateTime)
        {
            // Ensure the input is treated as UTC before converting
            if (utcDateTime.Kind == DateTimeKind.Local)
            {
                // Convert local to UTC first
                utcDateTime = utcDateTime.ToUniversalTime();
            }

            if (utcDateTime.Kind == DateTimeKind.Unspecified)
            {
                // Assume unspecified is UTC
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _localTimeZone);
        }

        public DateTime ConvertToUtc(DateTime localDateTime)
        {
            // If input is UTC already, return as-is
            if (localDateTime.Kind == DateTimeKind.Utc)
            {
                return localDateTime;
            }

            // If it's Local, convert to the target timezone by first converting to UTC
            if (localDateTime.Kind == DateTimeKind.Local)
            {
                var utc = localDateTime.ToUniversalTime();
                return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Utc).ToUniversalTime();
            }

            // Unspecified: assume it's in the configured local timezone
            var unspecifiedAsLocal = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
            // Use ConvertTimeToUtc with the configured timezone
            return TimeZoneInfo.ConvertTimeToUtc(unspecifiedAsLocal, _localTimeZone);
        }

        public TimeZoneInfo LocalTimeZone => _localTimeZone;
    }
}