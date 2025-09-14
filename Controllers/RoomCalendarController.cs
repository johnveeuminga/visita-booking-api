using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/rooms/{roomId:int}/calendar")]
    public class RoomCalendarController : ControllerBase
    {
        private readonly IRoomCalendarService _calendarService;
        private readonly ILogger<RoomCalendarController> _logger;

        public RoomCalendarController(
            IRoomCalendarService calendarService,
            ILogger<RoomCalendarController> logger)
        {
            _calendarService = calendarService;
            _logger = logger;
        }

        /// <summary>
        /// Get calendar view for a specific month
        /// </summary>
        [HttpGet("{year:int}/{month:int}")]
        public async Task<ActionResult<CalendarMonthDTO>> GetCalendarMonth(
            int roomId, 
            int year, 
            int month)
        {
            try
            {
                if (month < 1 || month > 12)
                    return BadRequest("Month must be between 1 and 12");

                if (year < DateTime.Now.Year || year > DateTime.Now.Year + 10)
                    return BadRequest("Invalid year");

                var calendar = await _calendarService.GetCalendarMonthAsync(roomId, year, month);
                return Ok(calendar);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting calendar for room {RoomId}, {Year}-{Month}", roomId, year, month);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get availability for a date range
        /// </summary>
        [HttpGet("availability")]
        public async Task<ActionResult<RoomAvailabilityRangeDTO>> GetAvailabilityRange(
            int roomId,
            [FromQuery, Required] DateTime startDate,
            [FromQuery, Required] DateTime endDate)
        {
            try
            {
                if (!await _calendarService.ValidateDateRangeAsync(startDate, endDate))
                    return BadRequest("Invalid date range");

                var availability = await _calendarService.GetAvailabilityRangeAsync(roomId, startDate, endDate);
                return Ok(availability);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting availability for room {RoomId} from {StartDate} to {EndDate}", 
                    roomId, startDate, endDate);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check if room is available on specific date
        /// </summary>
        [HttpGet("availability/{date}")]
        public async Task<ActionResult<object>> CheckAvailability(int roomId, DateTime date)
        {
            try
            {
                var isAvailable = await _calendarService.IsAvailableAsync(roomId, date);
                var price = await _calendarService.GetPriceForDateAsync(roomId, date);

                return Ok(new
                {
                    roomId,
                    date = date.Date,
                    isAvailable,
                    price
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for room {RoomId} on {Date}", roomId, date);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Set availability override for a specific date
        /// </summary>
        [HttpPost("availability")]
        [Authorize(Policy = "HotelOrAdminPolicy")]
        public async Task<ActionResult<RoomAvailabilityOverrideDTO>> SetAvailabilityOverride(
            int roomId,
            [FromBody] RoomAvailabilityOverrideCreateDTO overrideDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _calendarService.SetAvailabilityOverrideAsync(roomId, overrideDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting availability override for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Remove availability override for a specific date
        /// </summary>
        [HttpDelete("availability/{date}")]
        [Authorize(Policy = "HotelOrAdminPolicy")]
        public async Task<ActionResult> RemoveAvailabilityOverride(int roomId, DateTime date)
        {
            try
            {
                var success = await _calendarService.RemoveAvailabilityOverrideAsync(roomId, date);
                if (!success)
                    return NotFound("No override found for the specified date");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing availability override for room {RoomId} on {Date}", roomId, date);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Bulk update availability for multiple dates
        /// </summary>
        [HttpPost("availability/bulk")]
        [Authorize(Policy = "HotelOrAdminPolicy")]
        public async Task<ActionResult<object>> BulkSetAvailability(
            int roomId,
            [FromBody] BulkAvailabilityUpdateDTO bulkDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedCount = await _calendarService.BulkSetAvailabilityAsync(roomId, bulkDto);
                return Ok(new { updatedCount, message = $"Updated {updatedCount} dates" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating availability for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Bulk update availability for a date range
        /// </summary>
        [HttpPost("availability/range")]
        [Authorize(Policy = "HotelOrAdminPolicy")]
        public async Task<ActionResult<object>> BulkSetAvailabilityRange(
            int roomId,
            [FromBody] DateRangeAvailabilityUpdateDTO rangeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedCount = await _calendarService.BulkSetAvailabilityRangeAsync(roomId, rangeDto);
                return Ok(new { updatedCount, message = $"Updated {updatedCount} dates" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating availability range for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Clear all overrides for a date range
        /// </summary>
        [HttpDelete("availability")]
        [Authorize(Policy = "HotelOrAdminPolicy")]
        public async Task<ActionResult<object>> ClearAvailabilityOverrides(
            int roomId,
            [FromQuery, Required] DateTime startDate,
            [FromQuery, Required] DateTime endDate)
        {
            try
            {
                if (!await _calendarService.ValidateDateRangeAsync(startDate, endDate))
                    return BadRequest("Invalid date range");

                var clearedCount = await _calendarService.BulkClearOverridesAsync(roomId, startDate, endDate);
                return Ok(new { clearedCount, message = $"Cleared {clearedCount} overrides" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing availability overrides for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get pricing rules for the room
        /// </summary>
        [HttpGet("pricing-rules")]
        public async Task<ActionResult<List<RoomPricingRuleDTO>>> GetPricingRules(
            int roomId,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var rules = await _calendarService.GetPricingRulesAsync(roomId, includeInactive);
                return Ok(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pricing rules for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new pricing rule
        /// </summary>
        [HttpPost("pricing-rules")]
        [Authorize(Policy = "HotelOrAdminPolicy")]
        public async Task<ActionResult<RoomPricingRuleDTO>> CreatePricingRule(
            int roomId,
            [FromBody] RoomPricingRuleCreateDTO ruleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var rule = await _calendarService.CreatePricingRuleAsync(roomId, ruleDto);
                return CreatedAtAction(nameof(GetPricingRules), new { roomId }, rule);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pricing rule for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update a pricing rule
        /// </summary>
        [HttpPut("pricing-rules/{ruleId:int}")]
        [Authorize(Policy = "HotelOrAdminPolicy")]
        public async Task<ActionResult<RoomPricingRuleDTO>> UpdatePricingRule(
            int roomId,
            int ruleId,
            [FromBody] RoomPricingRuleCreateDTO ruleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var rule = await _calendarService.UpdatePricingRuleAsync(ruleId, ruleDto);
                if (rule == null)
                    return NotFound("Pricing rule not found");

                return Ok(rule);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pricing rule {RuleId} for room {RoomId}", ruleId, roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a pricing rule
        /// </summary>
        [HttpDelete("pricing-rules/{ruleId:int}")]
        [Authorize(Policy = "HotelOrAdminPolicy")]
        public async Task<ActionResult> DeletePricingRule(int roomId, int ruleId)
        {
            try
            {
                var success = await _calendarService.DeletePricingRuleAsync(ruleId);
                if (!success)
                    return NotFound("Pricing rule not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting pricing rule {RuleId} for room {RoomId}", ruleId, roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Toggle pricing rule active status
        /// </summary>
        [HttpPatch("pricing-rules/{ruleId:int}/toggle")]
        [Authorize(Policy = "HotelOrAdminPolicy")]
        public async Task<ActionResult> TogglePricingRule(int roomId, int ruleId)
        {
            try
            {
                var success = await _calendarService.TogglePricingRuleAsync(ruleId);
                if (!success)
                    return NotFound("Pricing rule not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling pricing rule {RuleId} for room {RoomId}", ruleId, roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Calculate total cost for a stay
        /// </summary>
        [HttpGet("pricing/calculate")]
        public async Task<ActionResult<object>> CalculateStayTotal(
            int roomId,
            [FromQuery, Required] DateTime checkIn,
            [FromQuery, Required] DateTime checkOut)
        {
            try
            {
                if (checkOut <= checkIn)
                    return BadRequest("Check-out date must be after check-in date");

                var total = await _calendarService.CalculateStayTotalAsync(roomId, checkIn, checkOut);
                var nights = (checkOut - checkIn).Days;
                var averageRate = total / nights;

                // Check minimum stay requirements
                var isValidStay = await _calendarService.ValidateMinimumStayAsync(roomId, checkIn, checkOut);

                return Ok(new
                {
                    roomId,
                    checkIn = checkIn.Date,
                    checkOut = checkOut.Date,
                    nights,
                    total,
                    averageRate,
                    isValidStay
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating stay total for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get pricing for a date range
        /// </summary>
        [HttpGet("pricing")]
        public async Task<ActionResult<Dictionary<DateTime, decimal>>> GetPricingRange(
            int roomId,
            [FromQuery, Required] DateTime startDate,
            [FromQuery, Required] DateTime endDate)
        {
            try
            {
                if (!await _calendarService.ValidateDateRangeAsync(startDate, endDate))
                    return BadRequest("Invalid date range");

                var prices = await _calendarService.GetPricesForRangeAsync(roomId, startDate, endDate);
                return Ok(prices);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pricing range for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get calendar statistics for a month
        /// </summary>
        [HttpGet("{year:int}/{month:int}/stats")]
        public async Task<ActionResult<Dictionary<string, object>>> GetCalendarStats(
            int roomId,
            int year,
            int month)
        {
            try
            {
                if (month < 1 || month > 12)
                    return BadRequest("Month must be between 1 and 12");

                var stats = await _calendarService.GetCalendarStatsAsync(roomId, year, month);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting calendar stats for room {RoomId}, {Year}-{Month}", roomId, year, month);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get holidays for a date range
        /// </summary>
        [HttpGet("holidays")]
        public async Task<ActionResult<object>> GetHolidays(
            int roomId,
            [FromQuery, Required] DateTime startDate,
            [FromQuery, Required] DateTime endDate,
            [FromQuery] string country = "US")
        {
            try
            {
                if (!await _calendarService.ValidateDateRangeAsync(startDate, endDate))
                    return BadRequest("Invalid date range");

                var holidays = await _calendarService.GetHolidaysAsync(startDate, endDate, country);
                return Ok(holidays.Select(h => new
                {
                    h.Date,
                    h.Name,
                    h.Description,
                    h.PriceMultiplier,
                    h.HolidayType,
                    h.IsNational
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting holidays for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Warm up cache for a date range
        /// </summary>
        [HttpPost("cache/warmup")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult> WarmupCache(
            int roomId,
            [FromQuery, Required] DateTime startDate,
            [FromQuery, Required] DateTime endDate)
        {
            try
            {
                if (!await _calendarService.ValidateDateRangeAsync(startDate, endDate))
                    return BadRequest("Invalid date range");

                await _calendarService.WarmupCalendarCacheAsync(roomId, startDate, endDate);
                return Ok(new { message = "Cache warmed up successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up cache for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Invalidate cache for the room
        /// </summary>
        [HttpDelete("cache")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult> InvalidateCache(int roomId)
        {
            try
            {
                await _calendarService.InvalidateCalendarCacheAsync(roomId);
                return Ok(new { message = "Cache invalidated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache for room {RoomId}", roomId);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    /// <summary>
    /// Search controller extension for calendar integration
    /// </summary>
    [ApiController]
    [Route("api/rooms/search")]
    public class RoomSearchCalendarController : ControllerBase
    {
        private readonly IRoomCalendarService _calendarService;
        private readonly ILogger<RoomSearchCalendarController> _logger;

        public RoomSearchCalendarController(
            IRoomCalendarService calendarService,
            ILogger<RoomSearchCalendarController> logger)
        {
            _calendarService = calendarService;
            _logger = logger;
        }

        /// <summary>
        /// Get available rooms for a date range
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<object>> GetAvailableRooms(
            [FromQuery, Required] DateTime checkIn,
            [FromQuery, Required] DateTime checkOut,
            [FromQuery] List<int>? roomIds = null)
        {
            try
            {
                if (checkOut <= checkIn)
                    return BadRequest("Check-out date must be after check-in date");

                if (!await _calendarService.ValidateDateRangeAsync(checkIn, checkOut))
                    return BadRequest("Invalid date range");

                var availableRoomIds = await _calendarService.GetAvailableRoomIdsAsync(checkIn, checkOut, roomIds);
                var roomPrices = await _calendarService.GetRoomPricesAsync(availableRoomIds, checkIn, checkOut);

                var results = availableRoomIds.Select(roomId => new
                {
                    RoomId = roomId,
                    TotalPrice = roomPrices.GetValueOrDefault(roomId, 0),
                    Nights = (checkOut - checkIn).Days,
                    AverageRate = roomPrices.GetValueOrDefault(roomId, 0) / Math.Max(1, (checkOut - checkIn).Days)
                }).OrderBy(r => r.TotalPrice);

                return Ok(new
                {
                    CheckIn = checkIn.Date,
                    CheckOut = checkOut.Date,
                    TotalRooms = availableRoomIds.Count,
                    Rooms = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for available rooms");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get available rooms for a date range with pagination
        /// </summary>
        [HttpGet("available/paginated")]
        public async Task<ActionResult<object>> GetAvailableRoomsPaginated(
            [FromQuery, Required] DateTime checkIn,
            [FromQuery, Required] DateTime checkOut,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] List<int>? roomIds = null)
        {
            try
            {
                if (checkOut <= checkIn)
                    return BadRequest("Check-out date must be after check-in date");

                if (!await _calendarService.ValidateDateRangeAsync(checkIn, checkOut))
                    return BadRequest("Invalid date range");

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var availableRoomsResult = await _calendarService.GetAvailableRoomIdsAsync(checkIn, checkOut, page, pageSize, roomIds);
                var roomPrices = await _calendarService.GetRoomPricesAsync(availableRoomsResult.Items, checkIn, checkOut);

                var rooms = availableRoomsResult.Items.Select(roomId => new
                {
                    RoomId = roomId,
                    TotalPrice = roomPrices.GetValueOrDefault(roomId, 0),
                    Nights = (checkOut - checkIn).Days,
                    AverageRate = roomPrices.GetValueOrDefault(roomId, 0) / Math.Max(1, (checkOut - checkIn).Days)
                }).OrderBy(r => r.TotalPrice);

                return Ok(new
                {
                    CheckIn = checkIn.Date,
                    CheckOut = checkOut.Date,
                    Page = availableRoomsResult.Page,
                    PageSize = availableRoomsResult.PageSize,
                    TotalCount = availableRoomsResult.TotalCount,
                    TotalPages = availableRoomsResult.TotalPages,
                    HasPreviousPage = availableRoomsResult.HasPreviousPage,
                    HasNextPage = availableRoomsResult.HasNextPage,
                    Rooms = rooms
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for available rooms with pagination");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get pricing for multiple rooms
        /// </summary>
        [HttpPost("pricing")]
        public async Task<ActionResult<Dictionary<int, decimal>>> GetRoomsPricing(
            [FromQuery, Required] DateTime checkIn,
            [FromQuery, Required] DateTime checkOut,
            [FromBody] List<int> roomIds)
        {
            try
            {
                if (checkOut <= checkIn)
                    return BadRequest("Check-out date must be after check-in date");

                if (!roomIds.Any())
                    return BadRequest("Room IDs are required");

                var prices = await _calendarService.GetRoomPricesAsync(roomIds, checkIn, checkOut);
                return Ok(prices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room pricing");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}