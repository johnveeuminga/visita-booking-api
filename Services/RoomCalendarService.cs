using Microsoft.EntityFrameworkCore;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;
using VisitaBookingApi.Data;
using visita_booking_api.Services.Interfaces;
using System.Globalization;

namespace visita_booking_api.Services
{
    public class RoomCalendarService : IRoomCalendarService
    {
    private readonly ApplicationDbContext _context;
    private readonly ICacheInvalidationService _cacheInvalidation;
    private readonly ILogger<RoomCalendarService> _logger;
    private readonly IConfiguration _configuration;
    private readonly visita_booking_api.Services.Interfaces.IAvailabilityLedgerService _ledgerService;

        public RoomCalendarService(
            ApplicationDbContext context,
            ICacheInvalidationService cacheInvalidation,
            ILogger<RoomCalendarService> logger,
            IConfiguration configuration,
            visita_booking_api.Services.Interfaces.IAvailabilityLedgerService ledgerService)
        {
            _context = context;
            _cacheInvalidation = cacheInvalidation;
            _logger = logger;
            _configuration = configuration;
            _ledgerService = ledgerService;
        }

        // Calendar Views
        public async Task<CalendarMonthDTO> GetCalendarMonthAsync(int roomId, int year, int month)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
                throw new ArgumentException($"Room with ID {roomId} not found");

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get all overrides for the month
            var overrides = await _context.RoomAvailabilityOverrides
                .Where(o => o.RoomId == roomId && o.Date >= startDate && o.Date <= endDate)
                .ToDictionaryAsync(o => o.Date.Date);

            // Get holidays for the month
            var holidays = await _context.HolidayCalendar
                .Where(h => h.Date >= startDate && h.Date <= endDate && h.IsActive)
                .ToDictionaryAsync(h => h.Date.Date);

            // Get pricing rules
            var pricingRules = await GetApplicablePricingRulesAsync(roomId, startDate, endDate);

            var days = new List<CalendarDayDTO>();
            var availableDays = 0;
            var bookedDays = 0;
            var blockedDays = 0;
            var minPrice = decimal.MaxValue;
            var maxPrice = decimal.MinValue;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayDto = await CreateCalendarDayDTOAsync(roomId, date, overrides, holidays, pricingRules, room.DefaultPrice);
                days.Add(dayDto);

                // Update statistics
                if (dayDto.IsAvailable && !dayDto.IsPastDate)
                    availableDays++;
                else if (dayDto.IsBooked)
                    bookedDays++;
                else if (dayDto.IsBlocked)
                    blockedDays++;

                if (dayDto.Price.HasValue)
                {
                    if (dayDto.Price.Value < minPrice) minPrice = dayDto.Price.Value;
                    if (dayDto.Price.Value > maxPrice) maxPrice = dayDto.Price.Value;
                }
            }

            return new CalendarMonthDTO
            {
                RoomId = roomId,
                Year = year,
                Month = month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                Days = days,
                MinPrice = minPrice == decimal.MaxValue ? null : minPrice,
                MaxPrice = maxPrice == decimal.MinValue ? null : maxPrice,
                AvailableDays = availableDays,
                BookedDays = bookedDays,
                BlockedDays = blockedDays
            };
        }

        public async Task<RoomAvailabilityRangeDTO> GetAvailabilityRangeAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            if (!await ValidateDateRangeAsync(startDate, endDate))
                throw new ArgumentException("Invalid date range");

            var availability = await CheckAvailabilityAsync(roomId, startDate, endDate);
            var prices = await GetPricesForRangeAsync(roomId, startDate, endDate);

            var totalPrice = prices.Values.Sum();
            var averagePrice = prices.Values.Count > 0 ? prices.Values.Average() : 0;
            var availableDays = availability.Count(a => a.IsAvailable);
            var unavailableDays = availability.Count - availableDays;

            return new RoomAvailabilityRangeDTO
            {
                RoomId = roomId,
                StartDate = startDate,
                EndDate = endDate,
                Availability = availability,
                TotalPrice = totalPrice,
                AveragePrice = averagePrice,
                AvailableDays = availableDays,
                UnavailableDays = unavailableDays
            };
        }

        // Availability Management
        public async Task<List<RoomAvailabilityDTO>> CheckAvailabilityAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            var availability = new List<RoomAvailabilityDTO>();

            var overrides = await _context.RoomAvailabilityOverrides
                .Where(o => o.RoomId == roomId && o.Date >= startDate && o.Date <= endDate)
                .ToDictionaryAsync(o => o.Date.Date);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var isAvailable = true;
                var price = await GetPriceForDateAsync(roomId, date);
                string? notes = null;
                string? reason = null;

                if (overrides.TryGetValue(date.Date, out var overrideData))
                {
                    isAvailable = overrideData.IsAvailable;
                    if (overrideData.OverridePrice.HasValue)
                        price = overrideData.OverridePrice.Value;
                    notes = overrideData.Notes;
                    reason = overrideData.Reason;
                }

                availability.Add(new RoomAvailabilityDTO
                {
                    RoomId = roomId,
                    Date = date,
                    IsAvailable = isAvailable,
                    Price = price,
                    Notes = notes,
                    Reason = reason
                });
            }

            return availability;
        }

        public async Task<bool> IsAvailableAsync(int roomId, DateTime date)
        {
            var override_ = await _context.RoomAvailabilityOverrides
                .FirstOrDefaultAsync(o => o.RoomId == roomId && o.Date == date.Date);

            if (override_ != null)
                return override_.IsAvailable;

            // Check if room exists and is active
            var room = await _context.Rooms.FindAsync(roomId);
            return room?.IsActive ?? false;
        }

        public async Task<RoomAvailabilityOverrideDTO> SetAvailabilityOverrideAsync(int roomId, RoomAvailabilityOverrideCreateDTO overrideDto)
        {
            var existingOverride = await _context.RoomAvailabilityOverrides
                .FirstOrDefaultAsync(o => o.RoomId == roomId && o.Date == overrideDto.Date.Date);

            if (existingOverride != null)
            {
                // Update existing override
                existingOverride.IsAvailable = overrideDto.IsAvailable;
                existingOverride.OverridePrice = overrideDto.OverridePrice;
                existingOverride.Notes = overrideDto.Notes;
                existingOverride.Reason = overrideDto.Reason;
                existingOverride.UpdateTimestamp();
            }
            else
            {
                // Create new override
                existingOverride = new RoomAvailabilityOverride
                {
                    RoomId = roomId,
                    Date = overrideDto.Date.Date,
                    IsAvailable = overrideDto.IsAvailable,
                    OverridePrice = overrideDto.OverridePrice,
                    Notes = overrideDto.Notes,
                    Reason = overrideDto.Reason,
                    CreatedBy = "System", // TODO: Get from current user context
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.RoomAvailabilityOverrides.Add(existingOverride);
            }

            await _context.SaveChangesAsync();

            // Invalidate cache
            await InvalidateCalendarCacheAsync(roomId, overrideDto.Date);

            return new RoomAvailabilityOverrideDTO
            {
                Id = existingOverride.Id,
                RoomId = existingOverride.RoomId,
                Date = existingOverride.Date,
                IsAvailable = existingOverride.IsAvailable,
                OverridePrice = existingOverride.OverridePrice,
                Notes = existingOverride.Notes,
                Reason = existingOverride.Reason,
                CreatedAt = existingOverride.CreatedAt,
                CreatedBy = existingOverride.CreatedBy
            };
        }

        public async Task<bool> RemoveAvailabilityOverrideAsync(int roomId, DateTime date)
        {
            var override_ = await _context.RoomAvailabilityOverrides
                .FirstOrDefaultAsync(o => o.RoomId == roomId && o.Date == date.Date);

            if (override_ == null)
                return false;

            _context.RoomAvailabilityOverrides.Remove(override_);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await InvalidateCalendarCacheAsync(roomId, date);

            return true;
        }

        // Pricing Calculations
        public async Task<decimal> GetPriceForDateAsync(int roomId, DateTime date)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
                throw new ArgumentException($"Room with ID {roomId} not found");

            var basePrice = room.DefaultPrice;

            // Check for availability override with price override
            var availabilityOverride = await _context.RoomAvailabilityOverrides
                .FirstOrDefaultAsync(o => o.RoomId == roomId && o.Date == date.Date);

            if (availabilityOverride?.OverridePrice.HasValue == true)
                return availabilityOverride.OverridePrice.Value;

            // Get applicable pricing rules
            var pricingRules = await _context.RoomPricingRules
                .Where(r => r.RoomId == roomId && r.IsActive)
                .OrderByDescending(r => r.Priority)
                .ToListAsync();

            // Apply pricing rules
            foreach (var rule in pricingRules)
            {
                if (rule.IsValidForDate(date))
                {
                    return rule.CalculatePrice(basePrice);
                }
            }

            // Check for holiday pricing
            var holiday = await _context.HolidayCalendar
                .FirstOrDefaultAsync(h => h.Date == date.Date && h.IsActive);

            if (holiday != null)
                return basePrice * holiday.PriceMultiplier;

            return basePrice;
        }

        public async Task<Dictionary<DateTime, decimal>> GetPricesForRangeAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            var prices = new Dictionary<DateTime, decimal>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                prices[date] = await GetPriceForDateAsync(roomId, date);
            }

            return prices;
        }

        public async Task<decimal> CalculateStayTotalAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            if (checkOut <= checkIn)
                throw new ArgumentException("Check-out date must be after check-in date");

            var total = 0m;

            for (var date = checkIn; date < checkOut; date = date.AddDays(1))
            {
                total += await GetPriceForDateAsync(roomId, date);
            }

            return total;
        }

        // Helper methods
        private async Task<CalendarDayDTO> CreateCalendarDayDTOAsync(
            int roomId, 
            DateTime date, 
            Dictionary<DateTime, RoomAvailabilityOverride> overrides,
            Dictionary<DateTime, HolidayCalendar> holidays,
            List<RoomPricingRule> pricingRules,
            decimal basePrice)
        {
            var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
            var isHoliday = holidays.ContainsKey(date.Date);
            var isPastDate = date.Date < DateTime.Today;
            
            var isAvailable = true;
            var isBooked = false;
            var isBlocked = false;
            var price = basePrice;
            string? notes = null;
            string status = "available";

            // Check for overrides
            if (overrides.TryGetValue(date.Date, out var override_))
            {
                isAvailable = override_.IsAvailable;
                if (override_.OverridePrice.HasValue)
                    price = override_.OverridePrice.Value;
                notes = override_.Notes;
                
                if (!isAvailable)
                {
                    isBlocked = true;
                    status = "blocked";
                }
            }
            else
            {
                // Apply pricing rules
                foreach (var rule in pricingRules.OrderByDescending(r => r.Priority))
                {
                    if (rule.IsValidForDate(date))
                    {
                        price = rule.CalculatePrice(basePrice);
                        break;
                    }
                }

                // Apply holiday pricing
                if (isHoliday && holidays.TryGetValue(date.Date, out var holiday))
                {
                    price = basePrice * holiday.PriceMultiplier;
                }
            }

            if (isPastDate)
                status = "past";
            else if (isBooked)
                status = "booked";
            else if (!isAvailable)
                status = "blocked";

            return new CalendarDayDTO
            {
                Date = date,
                Day = date.Day,
                DayOfWeek = date.DayOfWeek.ToString(),
                IsAvailable = isAvailable,
                IsBooked = isBooked,
                IsBlocked = isBlocked,
                Price = price,
                Notes = notes,
                IsWeekend = isWeekend,
                IsHoliday = isHoliday,
                IsPastDate = isPastDate,
                Status = status
            };
        }

        private async Task<List<RoomPricingRule>> GetApplicablePricingRulesAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            return await _context.RoomPricingRules
                .Where(r => r.RoomId == roomId && r.IsActive)
                .Where(r => r.RuleType == PricingRuleType.Default || 
                           r.RuleType == PricingRuleType.Weekend ||
                           (r.StartDate.HasValue && r.EndDate.HasValue && 
                            r.StartDate.Value <= endDate && r.EndDate.Value >= startDate))
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
        }

        // Pricing Rules Management
        public async Task<List<RoomPricingRuleDTO>> GetPricingRulesAsync(int roomId, bool includeInactive = false)
        {
            var query = _context.RoomPricingRules.Where(r => r.RoomId == roomId);
            
            if (!includeInactive)
                query = query.Where(r => r.IsActive);

            var rules = await query.OrderBy(r => r.Priority).ToListAsync();

            return rules.Select(r => new RoomPricingRuleDTO
            {
                Id = r.Id,
                RoomId = r.RoomId,
                RuleType = r.RuleType.ToString(),
                Name = r.Name,
                Description = r.Description,
                DayOfWeek = r.DayOfWeek,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                FixedPrice = r.FixedPrice,
                IsActive = r.IsActive,
                Priority = r.Priority,
                MinimumNights = r.MinimumNights,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();
        }

        public async Task<RoomPricingRuleDTO> CreatePricingRuleAsync(int roomId, RoomPricingRuleCreateDTO ruleDto)
        {
            // Validate the rule
            var validationErrors = await ValidatePricingRuleAsync(roomId, ruleDto);
            if (validationErrors.Any())
                throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");

            if (!Enum.TryParse<PricingRuleType>(ruleDto.RuleType, out var ruleType))
                throw new ArgumentException("Invalid rule type");

            var rule = new RoomPricingRule
            {
                RoomId = roomId,
                RuleType = ruleType,
                Name = ruleDto.Name,
                Description = ruleDto.Description,
                DayOfWeek = ruleDto.DayOfWeek,
                StartDate = ruleDto.StartDate,
                EndDate = ruleDto.EndDate,
                FixedPrice = ruleDto.FixedPrice,
                Priority = ruleDto.Priority,
                MinimumNights = ruleDto.MinimumNights,
                IsActive = true
            };

            _context.RoomPricingRules.Add(rule);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cacheInvalidation.InvalidatePricingCacheAsync(roomId);

            return ConvertToRoomPricingRuleDTOAsync(rule);
        }

        public async Task<RoomPricingRuleDTO?> UpdatePricingRuleAsync(int ruleId, RoomPricingRuleCreateDTO ruleDto)
        {
            var rule = await _context.RoomPricingRules.FindAsync(ruleId);
            if (rule == null)
                return null;

            // Validate the rule
            var validationErrors = await ValidatePricingRuleAsync(rule.RoomId, ruleDto);
            if (validationErrors.Any())
                throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");

            if (!Enum.TryParse<PricingRuleType>(ruleDto.RuleType, out var ruleType))
                throw new ArgumentException("Invalid rule type");

            rule.RuleType = ruleType;
            rule.Name = ruleDto.Name;
            rule.Description = ruleDto.Description;
            rule.DayOfWeek = ruleDto.DayOfWeek;
            rule.StartDate = ruleDto.StartDate;
            rule.EndDate = ruleDto.EndDate;
            rule.FixedPrice = ruleDto.FixedPrice;
            rule.Priority = ruleDto.Priority;
            rule.MinimumNights = ruleDto.MinimumNights;
            rule.UpdateTimestamp();

            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cacheInvalidation.InvalidatePricingCacheAsync(rule.RoomId);

            return ConvertToRoomPricingRuleDTOAsync(rule);
        }

        public async Task<bool> DeletePricingRuleAsync(int ruleId)
        {
            var rule = await _context.RoomPricingRules.FindAsync(ruleId);
            if (rule == null)
                return false;

            var roomId = rule.RoomId;
            _context.RoomPricingRules.Remove(rule);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cacheInvalidation.InvalidatePricingCacheAsync(roomId);

            return true;
        }

        public async Task<bool> TogglePricingRuleAsync(int ruleId)
        {
            var rule = await _context.RoomPricingRules.FindAsync(ruleId);
            if (rule == null)
                return false;

            rule.IsActive = !rule.IsActive;
            rule.UpdateTimestamp();
            
            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cacheInvalidation.InvalidatePricingCacheAsync(rule.RoomId);

            return true;
        }

        // Bulk Operations
        public async Task<int> BulkSetAvailabilityAsync(int roomId, BulkAvailabilityUpdateDTO bulkDto)
        {
            var updatedCount = 0;

            foreach (var date in bulkDto.Dates)
            {
                var overrideDto = new RoomAvailabilityOverrideCreateDTO
                {
                    Date = date,
                    IsAvailable = bulkDto.IsAvailable,
                    OverridePrice = bulkDto.OverridePrice,
                    Notes = bulkDto.Notes,
                    Reason = bulkDto.Reason
                };

                await SetAvailabilityOverrideAsync(roomId, overrideDto);
                updatedCount++;
            }

            // After applying per-date overrides, warmup ledger for the affected dates and invalidate cache
            try
            {
                if (bulkDto.Dates != null && bulkDto.Dates.Any())
                {
                    var startDate = bulkDto.Dates.Min().Date;
                    var endDateExclusive = bulkDto.Dates.Max().Date.AddDays(1);
                    await _ledgerService.WarmupRoomLedgerAsync(roomId, startDate, endDateExclusive);
                    await _cacheInvalidation.InvalidateAvailabilityCacheAsync(roomId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warmup ledger after bulk per-date availability update for room {RoomId}", roomId);
            }

            return updatedCount;
        }

        public async Task<int> BulkSetAvailabilityRangeAsync(int roomId, DateRangeAvailabilityUpdateDTO rangeDto)
        {
            if (!await ValidateDateRangeAsync(rangeDto.StartDate, rangeDto.EndDate))
                throw new ArgumentException("Invalid date range");

            var updatedCount = 0;

            for (var date = rangeDto.StartDate; date <= rangeDto.EndDate; date = date.AddDays(1))
            {
                // Skip excluded days of week
                if (rangeDto.ExcludeDayOfWeek.Contains((int)date.DayOfWeek))
                    continue;

                var overrideDto = new RoomAvailabilityOverrideCreateDTO
                {
                    Date = date,
                    IsAvailable = rangeDto.IsAvailable,
                    OverridePrice = rangeDto.OverridePrice,
                    Notes = rangeDto.Notes,
                    Reason = rangeDto.Reason
                };

                await SetAvailabilityOverrideAsync(roomId, overrideDto);
                updatedCount++;
            }

            // After applying the overrides, invalidate ledger for the affected room/date range so searches using ledger get refreshed
            try
            {
                var startDate = rangeDto.StartDate.Date;
                // Warmup uses exclusive end date; add one day
                var endDateExclusive = rangeDto.EndDate.Date.AddDays(1);
                await _ledgerService.WarmupRoomLedgerAsync(roomId, startDate, endDateExclusive);
                await _cacheInvalidation.InvalidateAvailabilityCacheAsync(roomId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warmup ledger after bulk availability update for room {RoomId}", roomId);
            }

            return updatedCount;
        }

        public async Task<int> BulkClearOverridesAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            var overrides = await _context.RoomAvailabilityOverrides
                .Where(o => o.RoomId == roomId && o.Date >= startDate && o.Date <= endDate)
                .ToListAsync();

            var count = overrides.Count;
            _context.RoomAvailabilityOverrides.RemoveRange(overrides);
            await _context.SaveChangesAsync();

            // Invalidate cache for the entire range
            await InvalidateCalendarCacheAsync(roomId);

            return count;
        }

        // Search Integration
        public async Task<List<int>> GetAvailableRoomIdsAsync(DateTime checkIn, DateTime checkOut, List<int>? roomIds = null)
        {
            // Use database-level filtering for performance
            var blockedRoomIds = await GetBlockedRoomIdsForPeriodAsync(checkIn, checkOut, roomIds);
            
            var query = _context.Rooms.Where(r => r.IsActive);
            
            if (roomIds != null && roomIds.Any())
                query = query.Where(r => roomIds.Contains(r.Id));
            
            // Exclude blocked rooms at database level
            if (blockedRoomIds.Any())
                query = query.Where(r => !blockedRoomIds.Contains(r.Id));

            return await query.Select(r => r.Id).ToListAsync();
        }

        public async Task<PaginatedResponse<int>> GetAvailableRoomIdsAsync(
            DateTime checkIn, 
            DateTime checkOut, 
            int page = 1, 
            int pageSize = 50, 
            List<int>? roomIds = null)
        {
            // Get blocked rooms for the entire period in one query
            var blockedRoomIds = await GetBlockedRoomIdsForPeriodAsync(checkIn, checkOut, roomIds);
            
            var query = _context.Rooms.Where(r => r.IsActive);
            
            if (roomIds != null && roomIds.Any())
                query = query.Where(r => roomIds.Contains(r.Id));
            
            // Exclude blocked rooms at database level
            if (blockedRoomIds.Any())
                query = query.Where(r => !blockedRoomIds.Contains(r.Id));

            var totalCount = await query.CountAsync();
            
            var availableRoomIds = await query
                .Select(r => r.Id)
                .OrderBy(r => r)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PaginatedResponse<int>.Create(availableRoomIds, totalCount, page, pageSize);
        }

        /// <summary>
        /// Get room IDs that are UNAVAILABLE for the specified date range (negative filtering approach)
        /// More efficient when most rooms are available
        /// </summary>
        public async Task<List<int>> GetUnavailableRoomIdsAsync(DateTime checkIn, DateTime checkOut, List<int>? roomIds = null)
        {
            try
            {
                // Direct query to get blocked room IDs - much more efficient for exclusion filtering
                var blockedRoomIds = await GetBlockedRoomIdsForPeriodAsync(checkIn, checkOut, roomIds);
                
                _logger.LogDebug("GetUnavailableRoomIdsAsync: Found {BlockedCount} unavailable rooms for period {CheckIn} to {CheckOut}", 
                    blockedRoomIds.Count, checkIn.ToString("yyyy-MM-dd"), checkOut.ToString("yyyy-MM-dd"));
                
                return blockedRoomIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unavailable room IDs for period {CheckIn} to {CheckOut}", checkIn, checkOut);
                // Return empty list on error to avoid excluding all rooms
                return new List<int>();
            }
        }

        private async Task<List<int>> GetBlockedRoomIdsForPeriodAsync(DateTime checkIn, DateTime checkOut, List<int>? roomIds = null)
        {
            // Inventory-aware blocked room computation
            // For each room, for each date in [checkIn, checkOut), compute EffectiveInventory and UnitsConsumed
            // If any date has AvailableUnits <= 0 (i.e. UnitsConsumed >= EffectiveInventory), mark room blocked

            var blockedRoomIds = new List<int>();

            // Load candidate rooms and their total units
            var roomsQuery = _context.Rooms.AsQueryable();
            if (roomIds != null && roomIds.Any())
                roomsQuery = roomsQuery.Where(r => roomIds.Contains(r.Id));

            var rooms = await roomsQuery
                .Select(r => new { r.Id, r.TotalUnits })
                .ToListAsync();

            if (!rooms.Any())
                return blockedRoomIds;

            var roomIdSet = rooms.Select(r => r.Id).ToList();

            // Load overrides in range
            var overrides = await _context.RoomAvailabilityOverrides
                .Where(o => roomIdSet.Contains(o.RoomId) && o.Date >= checkIn && o.Date < checkOut)
                .ToListAsync();

            var overridesByRoom = overrides
                .GroupBy(o => o.RoomId)
                .ToDictionary(g => g.Key, g => g.ToDictionary(o => o.Date.Date, o => o));

            // Load confirmed bookings overlapping range
            var bookings = await _context.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled
                    && roomIdSet.Contains(b.RoomId)
                    && b.CheckInDate < checkOut
                    && b.CheckOutDate > checkIn)
                .Select(b => new { b.RoomId, b.CheckInDate, b.CheckOutDate, b.Quantity })
                .ToListAsync();

            // Load active reservations overlapping range
            var now = DateTime.UtcNow;
            var reservations = await _context.BookingReservations
                .Where(r => r.Status == ReservationStatus.Active
                    && r.ExpiresAt > now
                    && roomIdSet.Contains(r.RoomId)
                    && r.CheckInDate < checkOut
                    && r.CheckOutDate > checkIn)
                .Select(r => new { r.RoomId, r.CheckInDate, r.CheckOutDate, r.Quantity })
                .ToListAsync();

            // Load active locks overlapping range
            var locks = await _context.BookingAvailabilityLocks
                .Where(l => l.IsActive && l.ExpiresAt > now
                    && roomIdSet.Contains(l.RoomId)
                    && l.CheckInDate < checkOut
                    && l.CheckOutDate > checkIn)
                .Select(l => new { l.RoomId, l.CheckInDate, l.CheckOutDate, l.Quantity })
                .ToListAsync();

            // Helper: iterate each room and compute per-date usage
            foreach (var room in rooms)
            {
                var isBlocked = false;

                // Build a per-date dictionary of consumed units initialized to 0
                var days = new List<DateTime>();
                for (var d = checkIn.Date; d < checkOut.Date; d = d.AddDays(1))
                    days.Add(d);

                var consumed = days.ToDictionary(d => d, d => 0);

                // Aggregate bookings
                var roomBookings = bookings.Where(b => b.RoomId == room.Id);
                foreach (var b in roomBookings)
                {
                    var start = MaxDate(b.CheckInDate.Date, checkIn.Date);
                    var end = MinDate(b.CheckOutDate.Date, checkOut.Date);
                    for (var d = start; d < end; d = d.AddDays(1))
                    {
                        if (consumed.ContainsKey(d)) consumed[d] += b.Quantity;
                    }
                }

                // Aggregate reservations
                var roomReservations = reservations.Where(r => r.RoomId == room.Id);
                foreach (var r in roomReservations)
                {
                    var start = MaxDate(r.CheckInDate.Date, checkIn.Date);
                    var end = MinDate(r.CheckOutDate.Date, checkOut.Date);
                    for (var d = start; d < end; d = d.AddDays(1))
                    {
                        if (consumed.ContainsKey(d)) consumed[d] += r.Quantity;
                    }
                }

                // Aggregate locks
                var roomLocks = locks.Where(l => l.RoomId == room.Id);
                foreach (var l in roomLocks)
                {
                    var start = MaxDate(l.CheckInDate.Date, checkIn.Date);
                    var end = MinDate(l.CheckOutDate.Date, checkOut.Date);
                    for (var d = start; d < end; d = d.AddDays(1))
                    {
                        if (consumed.ContainsKey(d)) consumed[d] += l.Quantity;
                    }
                }

                // For each date check EffectiveInventory and compare
                var roomOverrides = overridesByRoom.ContainsKey(room.Id) ? overridesByRoom[room.Id] : new Dictionary<DateTime, Models.Entities.RoomAvailabilityOverride>();

                foreach (var d in days)
                {
                    int effectiveInventory;
                    if (roomOverrides.TryGetValue(d, out var ov))
                    {
                        if (ov.AvailableCount.HasValue)
                        {
                            effectiveInventory = ov.AvailableCount.Value; // authoritative
                        }
                        else if (!ov.IsAvailable)
                        {
                            effectiveInventory = 0;
                        }
                        else
                        {
                            effectiveInventory = room.TotalUnits;
                        }
                    }
                    else
                    {
                        effectiveInventory = room.TotalUnits;
                    }

                    var used = consumed[d];

                    // Calculate breakdown counts for debugging
                    var bookingsCount = roomBookings.Where(b => d >= b.CheckInDate.Date && d < b.CheckOutDate.Date).Sum(b => b.Quantity);
                    var reservationsCount = roomReservations.Where(r => d >= r.CheckInDate.Date && d < r.CheckOutDate.Date).Sum(r => r.Quantity);
                    var locksCount = roomLocks.Where(l => d >= l.CheckInDate.Date && d < l.CheckOutDate.Date).Sum(l => l.Quantity);

                    // Debug logging to help diagnose unexpected blocking
                    var dbgOvExists = roomOverrides.TryGetValue(d, out var dbgOv);
                    var dbgOvAvailable = dbgOvExists ? dbgOv?.AvailableCount : (int?)null;
                    var dbgOvIsAvailable = dbgOvExists ? dbgOv?.IsAvailable : (bool?)null;

                    _logger.LogDebug("[DEBUG] Room {RoomId} date {Date} effectiveInventory={EffectiveInventory} consumed={Consumed} (bookings={B}, reservations={R}, locks={L}) overrideAvailable={OverrideAvailable} overrideIsAvailable={OverrideIsAvailable}",
                        room.Id, d.ToString("yyyy-MM-dd"), effectiveInventory, used, bookingsCount, reservationsCount, locksCount,
                        dbgOvAvailable.HasValue ? dbgOvAvailable.Value.ToString() : "(none)", dbgOvIsAvailable.HasValue ? dbgOvIsAvailable.Value.ToString() : "(none)");

                    if (used >= effectiveInventory)
                    {
                        isBlocked = true;
                        break;
                    }
                }

                if (isBlocked)
                    blockedRoomIds.Add(room.Id);
            }

            return blockedRoomIds.Distinct().ToList();
        }

        // Utility helpers for date bounds
        private static DateTime MaxDate(DateTime a, DateTime b) => a > b ? a : b;
        private static DateTime MinDate(DateTime a, DateTime b) => a < b ? a : b;

        public async Task<Dictionary<int, decimal>> GetRoomPricesAsync(List<int> roomIds, DateTime checkIn, DateTime checkOut)
        {
            if (!roomIds.Any()) return new Dictionary<int, decimal>();

            // Optimize with bulk data fetching
            var roomPrices = new Dictionary<int, decimal>();

            // Get all room default prices in one query
            var roomDefaultPrices = await _context.Rooms
                .Where(r => roomIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.DefaultPrice);

            // Get all availability overrides for all rooms in one query
            var overrides = await _context.RoomAvailabilityOverrides
                .Where(o => roomIds.Contains(o.RoomId) && o.Date >= checkIn && o.Date < checkOut)
                .ToListAsync();

            // Get all pricing rules for all rooms in one query
            var pricingRules = await _context.RoomPricingRules
                .Where(r => roomIds.Contains(r.RoomId) && r.IsActive)
                .ToListAsync();

            // Get all holidays for the date range in one query
            var holidays = await _context.HolidayCalendar
                .Where(h => h.Date >= checkIn && h.Date < checkOut && h.IsActive)
                .ToDictionaryAsync(h => h.Date.Date);

            // Group data by room for efficient processing
            var roomOverrides = overrides.GroupBy(o => o.RoomId).ToDictionary(g => g.Key, g => g.ToDictionary(o => o.Date.Date));
            var roomRules = pricingRules.GroupBy(r => r.RoomId).ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.Priority).ToList());

            // Calculate prices for each room
            foreach (var roomId in roomIds)
            {
                if (!roomDefaultPrices.ContainsKey(roomId))
                {
                    _logger.LogWarning("Room {RoomId} not found, skipping price calculation", roomId);
                    continue;
                }

                try
                {
                    var total = 0m;
                    var basePrice = roomDefaultPrices[roomId];
                    var roomOverrideDict = roomOverrides.GetValueOrDefault(roomId, new Dictionary<DateTime, RoomAvailabilityOverride>());
                    var roomRuleList = roomRules.GetValueOrDefault(roomId, new List<RoomPricingRule>());

                    // Calculate price for each night
                    for (var date = checkIn; date < checkOut; date = date.AddDays(1))
                    {
                        var dayPrice = basePrice;

                        // Check for availability override with price override
                        if (roomOverrideDict.TryGetValue(date.Date, out var availabilityOverride) && 
                            availabilityOverride.OverridePrice.HasValue)
                        {
                            dayPrice = availabilityOverride.OverridePrice.Value;
                        }
                        else
                        {
                            // Apply pricing rules
                            foreach (var rule in roomRuleList)
                            {
                                if (rule.IsValidForDate(date))
                                {
                                    dayPrice = rule.CalculatePrice(basePrice);
                                    break;
                                }
                            }

                            // Apply holiday pricing
                            if (holidays.TryGetValue(date.Date, out var holiday))
                            {
                                dayPrice = basePrice * holiday.PriceMultiplier;
                            }
                        }

                        total += dayPrice;
                    }

                    roomPrices[roomId] = total;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to calculate price for room {RoomId}", roomId);
                }
            }

            return roomPrices;
        }

        /// <summary>
        /// Returns the minimum available units for each room across the date range [checkIn, checkOut).
        /// This is optimized for bulk checks from search code.
        /// </summary>
        public async Task<Dictionary<int, int>> GetMinAvailableUnitsForRoomsAsync(List<int> roomIds, DateTime checkIn, DateTime checkOut)
        {
            var result = new Dictionary<int, int>();
            if (!roomIds.Any()) return result;

            // Load room totals
            var roomTotals = await _context.Rooms
                .Where(r => roomIds.Contains(r.Id))
                .Select(r => new { r.Id, r.TotalUnits })
                .ToDictionaryAsync(r => r.Id, r => r.TotalUnits);

            // Load overrides
            var overrides = await _context.RoomAvailabilityOverrides
                .Where(o => roomIds.Contains(o.RoomId) && o.Date >= checkIn && o.Date < checkOut)
                .ToListAsync();

            var overridesByRoom = overrides.GroupBy(o => o.RoomId).ToDictionary(g => g.Key, g => g.ToDictionary(o => o.Date.Date, o => o));

            // Load bookings/reservations/locks similar to blocked method but aggregated per date
            var bookings = await _context.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled
                    && roomIds.Contains(b.RoomId)
                    && b.CheckInDate < checkOut
                    && b.CheckOutDate > checkIn)
                .Select(b => new { b.RoomId, b.CheckInDate, b.CheckOutDate, b.Quantity })
                .ToListAsync();

            var now = DateTime.UtcNow;
            var reservations = await _context.BookingReservations
                .Where(r => r.Status == ReservationStatus.Active
                    && r.ExpiresAt > now
                    && roomIds.Contains(r.RoomId)
                    && r.CheckInDate < checkOut
                    && r.CheckOutDate > checkIn)
                .Select(r => new { r.RoomId, r.CheckInDate, r.CheckOutDate, r.Quantity })
                .ToListAsync();

            var locks = await _context.BookingAvailabilityLocks
                .Where(l => l.IsActive && l.ExpiresAt > now
                    && roomIds.Contains(l.RoomId)
                    && l.CheckInDate < checkOut
                    && l.CheckOutDate > checkIn)
                .Select(l => new { l.RoomId, l.CheckInDate, l.CheckOutDate, l.Quantity })
                .ToListAsync();

            // For each room compute min available units
            foreach (var roomId in roomIds)
            {
                var total = roomTotals.GetValueOrDefault(roomId, 0);

                // Build per-date consumed map
                var days = new List<DateTime>();
                for (var d = checkIn.Date; d < checkOut.Date; d = d.AddDays(1)) days.Add(d);

                var consumed = days.ToDictionary(d => d, d => 0);

                foreach (var b in bookings.Where(b => b.RoomId == roomId))
                {
                    var start = MaxDate(b.CheckInDate.Date, checkIn.Date);
                    var end = MinDate(b.CheckOutDate.Date, checkOut.Date);
                    for (var d = start; d < end; d = d.AddDays(1)) if (consumed.ContainsKey(d)) consumed[d] += b.Quantity;
                }

                foreach (var r in reservations.Where(r => r.RoomId == roomId))
                {
                    var start = MaxDate(r.CheckInDate.Date, checkIn.Date);
                    var end = MinDate(r.CheckOutDate.Date, checkOut.Date);
                    for (var d = start; d < end; d = d.AddDays(1)) if (consumed.ContainsKey(d)) consumed[d] += r.Quantity;
                }

                foreach (var l in locks.Where(l => l.RoomId == roomId))
                {
                    var start = MaxDate(l.CheckInDate.Date, checkIn.Date);
                    var end = MinDate(l.CheckOutDate.Date, checkOut.Date);
                    for (var d = start; d < end; d = d.AddDays(1)) if (consumed.ContainsKey(d)) consumed[d] += l.Quantity;
                }

                // For each date compute effective inventory and available units
                var roomOverrides = overridesByRoom.ContainsKey(roomId) ? overridesByRoom[roomId] : new Dictionary<DateTime, Models.Entities.RoomAvailabilityOverride>();
                var minAvailable = int.MaxValue;

                foreach (var d in days)
                {
                    int effectiveInventory;
                    if (roomOverrides.TryGetValue(d, out var ov))
                    {
                        if (ov.AvailableCount.HasValue) effectiveInventory = ov.AvailableCount.Value;
                        else if (!ov.IsAvailable) effectiveInventory = 0;
                        else effectiveInventory = total;
                    }
                    else effectiveInventory = total;

                    var available = effectiveInventory - consumed[d];
                    if (available < minAvailable) minAvailable = available;
                }

                if (minAvailable == int.MaxValue) minAvailable = total; // no days -> full
                result[roomId] = Math.Max(0, minAvailable);
            }

            return result;
        }

        // Holiday Management
        public async Task<List<HolidayCalendar>> GetHolidaysAsync(DateTime startDate, DateTime endDate, string country = "US")
        {
            return await _context.HolidayCalendar
                .Where(h => h.Date >= startDate && h.Date <= endDate && h.Country == country && h.IsActive)
                .OrderBy(h => h.Date)
                .ToListAsync();
        }

        public async Task<bool> IsHolidayAsync(DateTime date, string country = "US")
        {
            return await _context.HolidayCalendar
                .AnyAsync(h => h.Date == date.Date && h.Country == country && h.IsActive);
        }

        // Statistics and Analytics
        public async Task<Dictionary<string, object>> GetCalendarStatsAsync(int roomId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var calendar = await GetCalendarMonthAsync(roomId, year, month);

            return new Dictionary<string, object>
            {
                ["TotalDays"] = calendar.Days.Count,
                ["AvailableDays"] = calendar.AvailableDays,
                ["BookedDays"] = calendar.BookedDays,
                ["BlockedDays"] = calendar.BlockedDays,
                ["MinPrice"] = calendar.MinPrice ?? 0,
                ["MaxPrice"] = calendar.MaxPrice ?? 0,
                ["AveragePrice"] = await GetAverageRateAsync(roomId, startDate, endDate),
                ["TotalRevenue"] = calendar.Days.Where(d => d.IsBooked).Sum(d => d.Price ?? 0),
                ["PotentialRevenue"] = calendar.Days.Where(d => d.IsAvailable).Sum(d => d.Price ?? 0)
            };
        }

        public async Task<decimal> GetAverageRateAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            var prices = await GetPricesForRangeAsync(roomId, startDate, endDate);
            return prices.Values.Any() ? prices.Values.Average() : 0;
        }

        public async Task<double> GetOccupancyRateAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            var availability = await CheckAvailabilityAsync(roomId, startDate, endDate);
            var totalDays = availability.Count;
            var availableDays = availability.Count(a => a.IsAvailable);
            var bookedDays = totalDays - availableDays; // Simplified - in reality you'd check actual bookings
            
            return totalDays > 0 ? (double)bookedDays / totalDays * 100 : 0;
        }

        // Cache Management
        public async Task InvalidateCalendarCacheAsync(int roomId)
        {
            await _cacheInvalidation.InvalidateAvailabilityCacheAsync(roomId);
            await _cacheInvalidation.InvalidatePricingCacheAsync(roomId);
        }

        public async Task InvalidateCalendarCacheAsync(int roomId, DateTime date)
        {
            await InvalidateCalendarCacheAsync(roomId);
        }

        public async Task WarmupCalendarCacheAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            // Pre-load commonly accessed data
            _ = await CheckAvailabilityAsync(roomId, startDate, endDate);
            _ = await GetPricesForRangeAsync(roomId, startDate, endDate);
        }

        // Validation
        public async Task<bool> ValidateDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
                return false;

            var maxAdvanceBookingDays = _configuration.GetValue<int>("RoomManagement:Calendar:MaxAdvanceBookingDays", 365);
            var maxFutureDate = DateTime.Today.AddDays(maxAdvanceBookingDays);

            if (endDate > maxFutureDate)
                return false;

            var minAdvanceBookingDays = _configuration.GetValue<int>("RoomManagement:Calendar:MinAdvanceBookingDays", 0);
            var minDate = DateTime.Today.AddDays(minAdvanceBookingDays);

            return startDate >= minDate;
        }

        public async Task<bool> ValidateMinimumStayAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var nights = (checkOut - checkIn).Days;

            var applicableRules = await _context.RoomPricingRules
                .Where(r => r.RoomId == roomId && r.IsActive && r.MinimumNights.HasValue)
                .Where(r => r.RuleType == PricingRuleType.Default || 
                           (r.StartDate.HasValue && r.EndDate.HasValue &&
                            r.StartDate.Value <= checkOut && r.EndDate.Value >= checkIn))
                .OrderByDescending(r => r.Priority)
                .ToListAsync();

            foreach (var rule in applicableRules)
            {
                if (rule.MinimumNights.HasValue && nights < rule.MinimumNights.Value)
                    return false;
            }

            return true;
        }

        public async Task<List<string>> ValidatePricingRuleAsync(int roomId, RoomPricingRuleCreateDTO ruleDto)
        {
            var errors = new List<string>();

            // Check if room exists
            if (!await _context.Rooms.AnyAsync(r => r.Id == roomId))
                errors.Add("Room not found");

            // Validate rule type
            if (!Enum.TryParse<PricingRuleType>(ruleDto.RuleType, out var ruleType))
                errors.Add("Invalid rule type");

            // Validate date range for applicable rule types
            if (ruleType == PricingRuleType.Seasonal || ruleType == PricingRuleType.SpecialEvent)
            {
                if (!ruleDto.StartDate.HasValue || !ruleDto.EndDate.HasValue)
                    errors.Add("Start and end dates are required for seasonal/special event rules");
                else if (ruleDto.StartDate >= ruleDto.EndDate)
                    errors.Add("Start date must be before end date");
            }

            // Validate day of week for weekend rules
            if (ruleType == PricingRuleType.Weekend)
            {
                if (!ruleDto.DayOfWeek.HasValue || ruleDto.DayOfWeek < 0 || ruleDto.DayOfWeek > 6)
                    errors.Add("Valid day of week (0-6) is required for weekend rules");
            }

            // Validate pricing configuration - with simplified pricing, we just need a fixed price
            if (ruleDto.FixedPrice <= 0)
                errors.Add("Fixed price must be greater than 0");

            return errors;
        }

        // Helper method
        private RoomPricingRuleDTO ConvertToRoomPricingRuleDTOAsync(RoomPricingRule rule)
        {
            return new RoomPricingRuleDTO
            {
                Id = rule.Id,
                RoomId = rule.RoomId,
                RuleType = rule.RuleType.ToString(),
                Name = rule.Name,
                Description = rule.Description,
                DayOfWeek = rule.DayOfWeek,
                StartDate = rule.StartDate,
                EndDate = rule.EndDate,
                FixedPrice = rule.FixedPrice,
                IsActive = rule.IsActive,
                Priority = rule.Priority,
                MinimumNights = rule.MinimumNights,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt
            };
        }
    }
}