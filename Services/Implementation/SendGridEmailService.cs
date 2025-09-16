using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Services.Implementation
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendBookingConfirmationAsync(Booking booking, decimal roomPrice, decimal adminFee)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("SendGrid API key not configured; skipping booking confirmation email for booking {BookingId}", booking.Id);
                return;
            }

            var client = new SendGridClient(apiKey);

            var fromEmail = _configuration["SendGrid:FromEmail"] ?? "no-reply@visita.example";
            var fromName = _configuration["SendGrid:FromName"] ?? "Visita";

            var msg = new SendGridMessage();
            msg.SetFrom(new EmailAddress(fromEmail, fromName));
            msg.AddTo(new EmailAddress(booking.GuestEmail, booking.GuestName));
            msg.SetSubject($"Booking Confirmed: {booking.BookingReference}");

            var builder = new StringBuilder();
            builder.AppendLine($"Hello {booking.GuestName},");
            builder.AppendLine();
            builder.AppendLine($"Your booking {booking.BookingReference} has been confirmed.");
            builder.AppendLine();
            builder.AppendLine("Accommodation & Room details:");
            builder.AppendLine($"- Room ID: {booking.RoomId}");
            if (booking.Room != null)
            {
                builder.AppendLine($"- Room name: {booking.Room.Name}");
                if (booking.Room.Accommodation != null)
                {
                    builder.AppendLine($"- Accommodation: {booking.Room.Accommodation.Name}");
                    builder.AppendLine($"- Address: {booking.Room.Accommodation.Address}");
                }
            }
            builder.AppendLine();
            builder.AppendLine("Pricing:");
            builder.AppendLine($"- Room price: {roomPrice:C}");
            builder.AppendLine($"- Admin fee: {adminFee:C}");
            builder.AppendLine($"- Total amount: {booking.TotalAmount:C}");
            builder.AppendLine();
            builder.AppendLine($"Check-in: {booking.CheckInDate:yyyy-MM-dd}");
            builder.AppendLine($"Check-out: {booking.CheckOutDate:yyyy-MM-dd}");
            builder.AppendLine();
            builder.AppendLine("Thank you for booking with us.");

            msg.AddContent(MimeType.Text, builder.ToString());

            try
            {
                var response = await client.SendEmailAsync(msg);
                _logger.LogInformation("Sent booking confirmation email for booking {BookingId} with status {StatusCode}", booking.Id, response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send booking confirmation email for booking {BookingId}", booking.Id);
            }
        }
    }
}
