using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Services.Interfaces
{
    public interface IRoomCalendarService
    {
        // Calendar Views
        Task<CalendarMonthDTO> GetCalendarMonthAsync(int roomId, int year, int month);
        Task<RoomAvailabilityRangeDTO> GetAvailabilityRangeAsync(int roomId, DateTime startDate, DateTime endDate);
        
        // Availability Management
        Task<List<RoomAvailabilityDTO>> CheckAvailabilityAsync(int roomId, DateTime startDate, DateTime endDate);
        Task<bool> IsAvailableAsync(int roomId, DateTime date);
        Task<RoomAvailabilityOverrideDTO> SetAvailabilityOverrideAsync(int roomId, RoomAvailabilityOverrideCreateDTO overrideDto);
        Task<bool> RemoveAvailabilityOverrideAsync(int roomId, DateTime date);
        
        // Pricing Calculations
        Task<decimal> GetPriceForDateAsync(int roomId, DateTime date);
        Task<Dictionary<DateTime, decimal>> GetPricesForRangeAsync(int roomId, DateTime startDate, DateTime endDate);
        Task<decimal> CalculateStayTotalAsync(int roomId, DateTime checkIn, DateTime checkOut);
        
        // Pricing Rules Management
        Task<List<RoomPricingRuleDTO>> GetPricingRulesAsync(int roomId, bool includeInactive = false);
        Task<RoomPricingRuleDTO> CreatePricingRuleAsync(int roomId, RoomPricingRuleCreateDTO ruleDto);
        Task<RoomPricingRuleDTO?> UpdatePricingRuleAsync(int ruleId, RoomPricingRuleCreateDTO ruleDto);
        Task<bool> DeletePricingRuleAsync(int ruleId);
        Task<bool> TogglePricingRuleAsync(int ruleId);
        
        // Bulk Operations
        Task<int> BulkSetAvailabilityAsync(int roomId, BulkAvailabilityUpdateDTO bulkDto);
        Task<int> BulkSetAvailabilityRangeAsync(int roomId, DateRangeAvailabilityUpdateDTO rangeDto);
        Task<int> BulkClearOverridesAsync(int roomId, DateTime startDate, DateTime endDate);
        
        // Search Integration
        Task<List<int>> GetAvailableRoomIdsAsync(DateTime checkIn, DateTime checkOut, List<int>? roomIds = null);
        Task<PaginatedResponse<int>> GetAvailableRoomIdsAsync(DateTime checkIn, DateTime checkOut, int page, int pageSize, List<int>? roomIds = null);
        
        /// <summary>
        /// Get room IDs that are UNAVAILABLE for the specified date range (negative filtering approach)
        /// More efficient when most rooms are available
        /// </summary>
        Task<List<int>> GetUnavailableRoomIdsAsync(DateTime checkIn, DateTime checkOut, List<int>? roomIds = null);
        
        Task<Dictionary<int, decimal>> GetRoomPricesAsync(List<int> roomIds, DateTime checkIn, DateTime checkOut);
        
        // Holiday Management
        Task<List<HolidayCalendar>> GetHolidaysAsync(DateTime startDate, DateTime endDate, string country = "US");
        Task<bool> IsHolidayAsync(DateTime date, string country = "US");
        
        // Statistics and Analytics
        Task<Dictionary<string, object>> GetCalendarStatsAsync(int roomId, int year, int month);
        Task<decimal> GetAverageRateAsync(int roomId, DateTime startDate, DateTime endDate);
        Task<double> GetOccupancyRateAsync(int roomId, DateTime startDate, DateTime endDate);
        
        // Cache Management
        Task InvalidateCalendarCacheAsync(int roomId);
        Task InvalidateCalendarCacheAsync(int roomId, DateTime date);
        Task WarmupCalendarCacheAsync(int roomId, DateTime startDate, DateTime endDate);
        
        // Validation
        Task<bool> ValidateDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> ValidateMinimumStayAsync(int roomId, DateTime checkIn, DateTime checkOut);
        Task<List<string>> ValidatePricingRuleAsync(int roomId, RoomPricingRuleCreateDTO ruleDto);
    }
}