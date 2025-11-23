using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Models;

namespace visita_booking_api.Services.Implementation
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(
            IConfiguration configuration,
            ILogger<SendGridEmailService> logger
        )
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendBookingConfirmationAsync(
            Booking booking,
            decimal roomPrice,
            decimal adminFee
        )
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning(
                    "SendGrid API key not configured; skipping booking confirmation email for booking {BookingId}",
                    booking.Id
                );
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
                _logger.LogInformation(
                    "Sent booking confirmation email for booking {BookingId} with status {StatusCode}",
                    booking.Id,
                    response.StatusCode
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send booking confirmation email for booking {BookingId}",
                    booking.Id
                );
            }

            // Send notification to accommodation owner (if available)
            try
            {
                var accomEmail = booking.Room?.Accommodation?.EmailAddress;
                if (!string.IsNullOrWhiteSpace(accomEmail))
                {
                    var ownerMsg = new SendGridMessage();
                    ownerMsg.SetFrom(new EmailAddress(fromEmail, fromName));
                    ownerMsg.AddTo(new EmailAddress(accomEmail));
                    ownerMsg.SetSubject($"New Booking Confirmed: {booking.BookingReference}");

                    var ob = new StringBuilder();
                    ob.AppendLine($"A new booking has been confirmed for your property.");
                    ob.AppendLine();
                    ob.AppendLine($"Booking reference: {booking.BookingReference}");
                    ob.AppendLine($"Guest: {booking.GuestName} <{booking.GuestEmail}>");
                    ob.AppendLine($"Phone: {booking.GuestPhone}");
                    ob.AppendLine($"Room ID: {booking.RoomId}");
                    if (booking.Room != null)
                        ob.AppendLine($"Room name: {booking.Room.Name}");
                    if (booking.Room?.Accommodation != null)
                        ob.AppendLine($"Accommodation: {booking.Room.Accommodation.Name}");
                    ob.AppendLine();
                    ob.AppendLine($"Check-in: {booking.CheckInDate:yyyy-MM-dd}");
                    ob.AppendLine($"Check-out: {booking.CheckOutDate:yyyy-MM-dd}");
                    ob.AppendLine();
                    ob.AppendLine("Pricing:");
                    ob.AppendLine($"- Room price: {roomPrice:C}");
                    ob.AppendLine($"- Admin fee: {adminFee:C}");
                    ob.AppendLine($"- Total amount: {booking.TotalAmount:C}");
                    ob.AppendLine();
                    ob.AppendLine("Please prepare for the guest's arrival.");

                    ownerMsg.AddContent(MimeType.Text, ob.ToString());

                    var ownerResp = await client.SendEmailAsync(ownerMsg);
                    _logger.LogInformation(
                        "Sent accommodation notification email for booking {BookingId} to {Email} with status {StatusCode}",
                        booking.Id,
                        accomEmail,
                        ownerResp.StatusCode
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send accommodation notification email for booking {BookingId}",
                    booking.Id
                );
            }
        }

        public async Task SendPasswordResetEmailAsync(
            string toEmail,
            string toName,
            string resetLink
        )
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning(
                    "SendGrid API key not configured; skipping password reset email to {Email}",
                    toEmail
                );
                return;
            }

            Console.WriteLine("apiKey: " + apiKey); // Debug line to check if apiKey is being read correctly

            var client = new SendGridClient(apiKey);

            var fromEmail = _configuration["SendGrid:FromEmail"] ?? "no-reply@visita.example";
            var fromName = _configuration["SendGrid:FromName"] ?? "Visita";

            var msg = new SendGridMessage();
            msg.SetFrom(new EmailAddress(fromEmail, fromName));
            msg.AddTo(new EmailAddress(toEmail, toName));
            msg.SetSubject("Your Password Reset Link");

            var builder = new StringBuilder();
            builder.AppendLine($"Hello {toName},");
            builder.AppendLine();
            builder.AppendLine(
                "We received a request to reset your password. Click the link below to set a new password. This link will expire in 1 hour."
            );
            builder.AppendLine();
            builder.AppendLine(resetLink);
            builder.AppendLine();
            builder.AppendLine(
                "If you didn't request a password reset, you can safely ignore this email."
            );
            builder.AppendLine();
            builder.AppendLine("Thanks,\nThe Visita Team");

            msg.AddContent(MimeType.Text, builder.ToString());

            try
            {
                var response = await client.SendEmailAsync(msg);
                _logger.LogInformation(
                    "Sent password reset email to {Email} with status {StatusCode}",
                    toEmail,
                    response.StatusCode
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            }
        }

        public async Task SendNewAccommodationAlertToAdminsAsync(
            Accommodation accommodation,
            User owner
        )
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning(
                    "SendGrid API key not configured; skipping admin notification for accommodation {AccommodationId}",
                    accommodation.Id
                );
                return;
            }

            var client = new SendGridClient(apiKey);

            var fromEmail = _configuration["SendGrid:FromEmail"] ?? "no-reply@visita.example";
            var fromName = _configuration["SendGrid:FromName"] ?? "Visita";

            var adminEmails = new List<string>
            {
                "jragudo311@gmail.com",
                "rapada.jhoniel@gmail.com",
            };

            foreach (var adminEmail in adminEmails)
            {
                if (string.IsNullOrWhiteSpace(adminEmail))
                    continue;

                var msg = new SendGridMessage();
                msg.SetFrom(new EmailAddress(fromEmail, fromName));
                msg.AddTo(new EmailAddress(adminEmail));
                msg.SetSubject("🏨 New Accommodation Registration - Pending Approval");

                var builder = new StringBuilder();
                builder.AppendLine("New Accommodation Registered");
                builder.AppendLine();
                builder.AppendLine(
                    "A new accommodation has been registered on Visita Booking and requires your approval."
                );
                builder.AppendLine();
                builder.AppendLine("=== Accommodation Details ===");
                builder.AppendLine($"Name: {accommodation.Name}");
                builder.AppendLine($"Location: {accommodation.Address ?? "Not specified"}");
                builder.AppendLine($"Address: {accommodation.Address ?? "Not specified"}");
                builder.AppendLine($"Status: Pending Approval");
                builder.AppendLine();
                builder.AppendLine("=== Registered By (Hotel Manager) ===");
                builder.AppendLine($"Name: {owner.FirstName} {owner.LastName}");
                builder.AppendLine($"Email: {owner.Email}");
                builder.AppendLine();
                builder.AppendLine("Please review and approve this accommodation at:");
                builder.AppendLine(
                    "https://booking.baguio.visita.ph/admin/accommodations?status=pending"
                );
                builder.AppendLine();
                builder.AppendLine("---");
                builder.AppendLine("This is an automated notification from Visita Booking System.");

                msg.AddContent(MimeType.Text, builder.ToString());

                try
                {
                    var response = await client.SendEmailAsync(msg);
                    _logger.LogInformation(
                        "Sent admin notification email for accommodation {AccommodationId} to {AdminEmail} with status {StatusCode}",
                        accommodation.Id,
                        adminEmail,
                        response.StatusCode
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send admin notification email for accommodation {AccommodationId} to {AdminEmail}",
                        accommodation.Id,
                        adminEmail
                    );
                }
            }
        }
    }
}
