using visita_booking_api.Models.DTOs;

namespace visita_booking_api.Services.Interfaces
{
    public interface IBulletinEventService
    {
        Task<BulletinCalendarMonth> GetCalendarMonthAsync(int year, int month);
        Task<BulletinEventResponse> GetEventByIdAsync(int id);
        Task<List<BulletinEventResponse>> GetAllEventsAsync();
        Task<BulletinEventResponse> CreateEventAsync(
            CreateBulletinEventRequest request,
            int? userId
        );
        Task<BulletinEventResponse> UpdateEventAsync(int id, UpdateBulletinEventRequest request);
        Task<bool> DeleteEventAsync(int id);
    }
}
