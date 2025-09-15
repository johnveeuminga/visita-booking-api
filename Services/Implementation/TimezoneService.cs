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
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
            {
                // Assume it's UTC if unspecified
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }
            
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _localTimeZone);
        }

        public DateTime ConvertToUtc(DateTime localDateTime)
        {
            if (localDateTime.Kind == DateTimeKind.Unspecified)
            {
                // Assume it's local time if unspecified
                localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Local);
            }

            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, _localTimeZone);
        }

        public TimeZoneInfo LocalTimeZone => _localTimeZone;
    }
}