using System.Threading.Tasks;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendBookingConfirmationAsync(Booking booking, decimal roomPrice, decimal adminFee);
    }
}
