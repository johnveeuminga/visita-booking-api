using Microsoft.EntityFrameworkCore;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;

namespace visita_booking_api.Services.Implementation
{
    public class BulletinEventService : IBulletinEventService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BulletinEventService> _logger;

        public BulletinEventService(
            ApplicationDbContext context,
            ILogger<BulletinEventService> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BulletinCalendarMonth> GetCalendarMonthAsync(int year, int month)
        {
            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            // Get all events that overlap with this month
            var events = await _context
                .BulletinEvents.Include(e => e.Creator)
                .Where(e => e.StartDate <= lastDay && e.EndDate >= firstDay)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            var calendarDays = new List<BulletinCalendarDay>();
            var today = DateTime.Today;

            for (int day = 1; day <= lastDay.Day; day++)
            {
                var currentDate = new DateTime(year, month, day);
                var dateStr = currentDate.ToString("yyyy-MM-dd");

                var dayEvents = events
                    .Where(e =>
                        e.StartDate.Date <= currentDate.Date && e.EndDate.Date >= currentDate.Date
                    )
                    .Select(e => new BulletinEventSummary
                    {
                        Id = e.Id,
                        Title = e.Title,
                        EventType = e.EventType,
                        IsMultiDay = e.StartDate.Date != e.EndDate.Date,
                        IsStartDate = e.StartDate.Date == currentDate.Date,
                        IsEndDate = e.EndDate.Date == currentDate.Date,
                    })
                    .ToList();

                calendarDays.Add(
                    new BulletinCalendarDay
                    {
                        Day = day,
                        Date = dateStr,
                        IsWeekend =
                            currentDate.DayOfWeek == DayOfWeek.Saturday
                            || currentDate.DayOfWeek == DayOfWeek.Sunday,
                        IsToday = currentDate.Date == today,
                        IsPastDate = currentDate.Date < today,
                        Events = dayEvents,
                    }
                );
            }

            return new BulletinCalendarMonth
            {
                Year = year,
                Month = month,
                MonthName = firstDay.ToString("MMMM"),
                Days = calendarDays,
                TotalEvents = events.Count,
            };
        }

        public async Task<BulletinEventResponse> GetEventByIdAsync(int id)
        {
            var bulletinEvent = await _context
                .BulletinEvents.Include(e => e.Creator)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (bulletinEvent == null)
            {
                throw new KeyNotFoundException($"Bulletin event with ID {id} not found");
            }

            return MapToResponse(bulletinEvent);
        }

        public async Task<List<BulletinEventResponse>> GetAllEventsAsync()
        {
            var events = await _context
                .BulletinEvents.Include(e => e.Creator)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            return events.Select(MapToResponse).ToList();
        }

        public async Task<BulletinEventResponse> CreateEventAsync(
            CreateBulletinEventRequest request,
            int? userId
        )
        {
            var bulletinEvent = new BulletinEvent
            {
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                EventType = request.EventType,
                LinkUrl = request.LinkUrl,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.BulletinEvents.Add(bulletinEvent);
            await _context.SaveChangesAsync();

            return await GetEventByIdAsync(bulletinEvent.Id);
        }

        public async Task<BulletinEventResponse> UpdateEventAsync(
            int id,
            UpdateBulletinEventRequest request
        )
        {
            var bulletinEvent = await _context.BulletinEvents.FindAsync(id);

            if (bulletinEvent == null)
            {
                throw new KeyNotFoundException($"Bulletin event with ID {id} not found");
            }

            bulletinEvent.Title = request.Title;
            bulletinEvent.Description = request.Description;
            bulletinEvent.StartDate = request.StartDate;
            bulletinEvent.EndDate = request.EndDate;
            bulletinEvent.EventType = request.EventType;
            bulletinEvent.LinkUrl = request.LinkUrl;
            bulletinEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetEventByIdAsync(id);
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var bulletinEvent = await _context.BulletinEvents.FindAsync(id);

            if (bulletinEvent == null)
            {
                return false;
            }

            _context.BulletinEvents.Remove(bulletinEvent);
            await _context.SaveChangesAsync();

            return true;
        }

        private BulletinEventResponse MapToResponse(BulletinEvent bulletinEvent)
        {
            return new BulletinEventResponse
            {
                Id = bulletinEvent.Id,
                Title = bulletinEvent.Title,
                Description = bulletinEvent.Description,
                StartDate = bulletinEvent.StartDate,
                EndDate = bulletinEvent.EndDate,
                EventType = bulletinEvent.EventType,
                LinkUrl = bulletinEvent.LinkUrl,
                CreatedBy = bulletinEvent.CreatedBy,
                CreatorName = bulletinEvent.Creator?.FullName,
                CreatedAt = bulletinEvent.CreatedAt,
                UpdatedAt = bulletinEvent.UpdatedAt,
            };
        }
    }
}
