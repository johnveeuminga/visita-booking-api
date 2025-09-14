using System.ComponentModel.DataAnnotations;

namespace visita_booking_api.Models.DTOs
{
    // Room Availability DTOs
    public class RoomAvailabilityDTO
    {
        public int RoomId { get; set; }
        public DateTime Date { get; set; }
        public bool IsAvailable { get; set; }
        public decimal? Price { get; set; }
        public string? Notes { get; set; }
        public string? Reason { get; set; }
    }

    public class RoomAvailabilityRangeDTO
    {
        public int RoomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<RoomAvailabilityDTO> Availability { get; set; } = new();
        public decimal? TotalPrice { get; set; }
        public decimal? AveragePrice { get; set; }
        public int AvailableDays { get; set; }
        public int UnavailableDays { get; set; }
    }

    public class RoomAvailabilityOverrideDTO
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public DateTime Date { get; set; }
        public bool IsAvailable { get; set; }
        public decimal? OverridePrice { get; set; }
        public string? Notes { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class RoomAvailabilityOverrideCreateDTO
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        [Range(0.01, 99999.99)]
        public decimal? OverridePrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? Reason { get; set; }
    }

    // Calendar DTOs
    public class CalendarMonthDTO
    {
        public int RoomId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public List<CalendarDayDTO> Days { get; set; } = new();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int AvailableDays { get; set; }
        public int BookedDays { get; set; }
        public int BlockedDays { get; set; }
    }

    public class CalendarDayDTO
    {
        public DateTime Date { get; set; }
        public int Day { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsBooked { get; set; }
        public bool IsBlocked { get; set; }
        public decimal? Price { get; set; }
        public string? Notes { get; set; }
        public bool IsWeekend { get; set; }
        public bool IsHoliday { get; set; }
        public bool IsPastDate { get; set; }
        public string Status { get; set; } = string.Empty; // "available", "booked", "blocked", "past"
    }

    // Bulk Calendar Update DTOs
    public class BulkAvailabilityUpdateDTO
    {
        [Required]
        public List<DateTime> Dates { get; set; } = new();

        [Required]
        public bool IsAvailable { get; set; }

        [Range(0.01, 99999.99)]
        public decimal? OverridePrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? Reason { get; set; }
    }

    public class DateRangeAvailabilityUpdateDTO
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        [Range(0.01, 99999.99)]
        public decimal? OverridePrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? Reason { get; set; }

        public List<int> ExcludeDayOfWeek { get; set; } = new(); // 0=Sunday, 1=Monday, etc.
    }
}