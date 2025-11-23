using System.Threading.Tasks;
using visita_booking_api.Models.Entities;
using VisitaBookingApi.Models;

namespace visita_booking_api.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendBookingConfirmationAsync(Booking booking, decimal roomPrice, decimal adminFee);
        Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink);

        Task SendNewAccommodationAlertToAdminsAsync(Accommodation accommodation, User owner);
    }
}
