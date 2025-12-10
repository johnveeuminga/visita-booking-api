namespace visita_booking_api.Models.DTOs
{
    // Request DTOs
    public class CreateBulletinEventRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
    }

    public class UpdateBulletinEventRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
    }

    // Response DTOs
    public class BulletinEventResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BulletinCalendarMonth
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public List<BulletinCalendarDay> Days { get; set; } = new();
        public int TotalEvents { get; set; }
    }

    public class BulletinCalendarDay
    {
        public int Day { get; set; }
        public string Date { get; set; } = string.Empty;
        public bool IsWeekend { get; set; }
        public bool IsToday { get; set; }
        public bool IsPastDate { get; set; }
        public List<BulletinEventSummary> Events { get; set; } = new();
        public bool HasEvents => Events.Any();
    }

    public class BulletinEventSummary
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public bool IsMultiDay { get; set; }
        public bool IsStartDate { get; set; }
        public bool IsEndDate { get; set; }
    }
}
