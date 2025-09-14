namespace visita_booking_api.Models.Cache
{
    public static class CacheKeys
    {
        public const string ROOM_DETAILS_PREFIX = "room:details:";
        public const string ROOM_PHOTOS_PREFIX = "room:photos:";
        public const string ROOM_AMENITIES_PREFIX = "room:amenities:";
        public const string ROOM_AVAILABILITY_PREFIX = "room:availability:";
        public const string ROOM_PRICING_PREFIX = "room:pricing:";
        public const string ROOM_CALENDAR_PREFIX = "room:calendar:";
        public const string ROOM_PRICING_RULES_PREFIX = "room:pricing-rules:";
        public const string ROOM_STATS_PREFIX = "room:stats:";
        public const string SEARCH_RESULTS_PREFIX = "search:results:";
        public const string SEARCH_AVAILABLE_ROOMS_PREFIX = "search:available:";
        public const string AMENITIES_ALL = "amenities:all";
        public const string AMENITY_DETAILS_PREFIX = "amenity:details:";
        public const string AMENITY_CATEGORIES = "amenity:categories";
        public const string CALENDAR_MATRIX_PREFIX = "calendar:matrix:";
        public const string HOLIDAY_CALENDAR_PREFIX = "holidays:";
        public const string POPULAR_ROOMS = "popular:rooms";

        // Cache key generators
        public static string RoomDetails(int roomId) => $"{ROOM_DETAILS_PREFIX}{roomId}";
        public static string RoomPhotos(int roomId) => $"{ROOM_PHOTOS_PREFIX}{roomId}";
        public static string RoomAmenities(int roomId) => $"{ROOM_AMENITIES_PREFIX}{roomId}";
        public static string RoomAvailability(int roomId, string dateRange) => $"{ROOM_AVAILABILITY_PREFIX}{roomId}:{dateRange}";
        public static string RoomPricing(int roomId, string dateRange) => $"{ROOM_PRICING_PREFIX}{roomId}:{dateRange}";
        public static string RoomCalendar(int roomId, int year, int month) => $"{ROOM_CALENDAR_PREFIX}{roomId}:{year}-{month:D2}";
        public static string RoomPricingRules(int roomId) => $"{ROOM_PRICING_RULES_PREFIX}{roomId}";
        public static string RoomStats(int roomId, int year, int month) => $"{ROOM_STATS_PREFIX}{roomId}:{year}-{month:D2}";
        public static string SearchResults(string searchHash) => $"{SEARCH_RESULTS_PREFIX}{searchHash}";
        public static string SearchAvailableRooms(DateTime checkIn, DateTime checkOut) => $"{SEARCH_AVAILABLE_ROOMS_PREFIX}{checkIn:yyyyMMdd}-{checkOut:yyyyMMdd}";
        public static string CalendarMatrix(int roomId, string yearMonth) => $"{CALENDAR_MATRIX_PREFIX}{roomId}:{yearMonth}";
        public static string HolidayCalendar(string country, int year) => $"{HOLIDAY_CALENDAR_PREFIX}{country}:{year}";
        public static string AmenityDetails(int amenityId) => $"{AMENITY_DETAILS_PREFIX}{amenityId}";

        // Pattern generators for bulk operations
        public static string RoomPattern(int roomId) => $"room:*:{roomId}*";
        public static string SearchPattern() => $"{SEARCH_RESULTS_PREFIX}*";
        public static string AvailabilityPattern(int roomId) => $"{ROOM_AVAILABILITY_PREFIX}{roomId}:*";
        public static string CalendarPattern(int roomId) => $"{ROOM_CALENDAR_PREFIX}{roomId}:*";
        public static string PricingPattern(int roomId) => $"{ROOM_PRICING_PREFIX}{roomId}:*";
    }

    public class CacheSettings
    {
        // TTL configurations (in seconds)
        public static class TTL
        {
            public const int ROOM_DETAILS = 3600; // 1 hour
            public const int ROOM_PHOTOS = 86400; // 24 hours
            public const int AMENITIES_ALL = 86400; // 24 hours
            public const int AMENITY_DETAILS = 3600; // 1 hour
            public const int AMENITY_LIST = 3600; // 1 hour
            public const int AMENITY_LIST_ADMIN = 1800; // 30 minutes (includes inactive)
            public const int AMENITY_CATEGORIES = 86400; // 24 hours
            public const int AVAILABILITY_SHORT = 300; // 5 minutes
            public const int AVAILABILITY_LONG = 1800; // 30 minutes
            public const int PRICING_SHORT = 600; // 10 minutes
            public const int PRICING_LONG = 3600; // 1 hour
            public const int CALENDAR_MONTH = 300; // 5 minutes
            public const int CALENDAR_STATS = 1800; // 30 minutes
            public const int PRICING_RULES = 3600; // 1 hour
            public const int HOLIDAY_CALENDAR = 86400; // 24 hours
            public const int SEARCH_RESULTS = 180; // 3 minutes
            public const int SEARCH_AVAILABLE_ROOMS = 300; // 5 minutes
            public const int CALENDAR_MATRIX = 300; // 5 minutes
        }

        public static class Priority
        {
            public const int HIGH = 1;
            public const int MEDIUM = 2;
            public const int LOW = 3;
        }
    }
}