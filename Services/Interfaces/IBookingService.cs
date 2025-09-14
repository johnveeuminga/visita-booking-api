using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Services.Interfaces
{
    public interface IBookingService
    {
        /// <summary>
        /// Checks room availability for the specified dates
        /// </summary>
        /// <param name="request">Availability request parameters</param>
        /// <returns>Availability information with pricing details</returns>
        Task<BookingAvailabilityResponseDto> CheckAvailabilityAsync(BookingAvailabilityRequestDto request);

        /// <summary>
        /// Creates a new booking reservation with temporary hold on the room
        /// </summary>
        /// <param name="request">Booking creation request</param>
        /// <param name="userId">ID of the user making the booking</param>
        /// <returns>Created booking with reservation details</returns>
        Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto request, int userId);

        /// <summary>
        /// Confirms a booking after successful payment
        /// </summary>
        /// <param name="bookingId">ID of the booking to confirm</param>
        /// <param name="paymentReference">Payment reference from payment provider</param>
        /// <returns>Updated booking information</returns>
        Task<BookingResponseDto> ConfirmBookingAsync(int bookingId, string paymentReference);

        /// <summary>
        /// Cancels a booking and initiates refund if applicable
        /// </summary>
        /// <param name="bookingId">ID of the booking to cancel</param>
        /// <param name="request">Cancellation request details</param>
        /// <param name="userId">ID of the user requesting cancellation</param>
        /// <returns>Cancelled booking information</returns>
        Task<BookingResponseDto> CancelBookingAsync(int bookingId, CancelBookingRequestDto request, int userId);

        /// <summary>
        /// Extends an active reservation by specified minutes
        /// </summary>
        /// <param name="request">Extension request details</param>
        /// <param name="userId">ID of the user requesting extension</param>
        /// <returns>Extended reservation information</returns>
        Task<BookingReservationDto> ExtendReservationAsync(ExtendReservationRequestDto request, int userId);

        /// <summary>
        /// Gets booking details by ID
        /// </summary>
        /// <param name="bookingId">ID of the booking</param>
        /// <param name="userId">ID of the requesting user (for authorization)</param>
        /// <returns>Booking details</returns>
        Task<BookingResponseDto?> GetBookingAsync(int bookingId, int? userId = null);

        /// <summary>
        /// Gets booking details by booking reference
        /// </summary>
        /// <param name="bookingReference">Booking reference number</param>
        /// <param name="userId">ID of the requesting user (for authorization)</param>
        /// <returns>Booking details</returns>
        Task<BookingResponseDto?> GetBookingByReferenceAsync(string bookingReference, int? userId = null);

        /// <summary>
        /// Searches bookings with filtering and pagination
        /// </summary>
        /// <param name="request">Search criteria</param>
        /// <param name="userId">ID of the requesting user (null for admin)</param>
        /// <returns>Paginated search results</returns>
        Task<PagedResult<BookingSummaryDto>> SearchBookingsAsync(BookingSearchRequestDto request, int? userId = null);

        /// <summary>
        /// Updates booking status (admin only)
        /// </summary>
        /// <param name="bookingId">ID of the booking</param>
        /// <param name="request">Status update request</param>
        /// <returns>Updated booking information</returns>
        Task<BookingResponseDto> UpdateBookingStatusAsync(int bookingId, UpdateBookingStatusDto request);

        /// <summary>
        /// Processes payment webhook from payment provider
        /// </summary>
        /// <param name="webhookData">Webhook payload</param>
        /// <param name="signature">Webhook signature for verification</param>
        /// <returns>Processing result</returns>
        Task<bool> ProcessPaymentWebhookAsync(string webhookData, string signature);

        /// <summary>
        /// Initiates refund for a booking payment
        /// </summary>
        /// <param name="bookingId">ID of the booking</param>
        /// <param name="request">Refund request details</param>
        /// <returns>Refund processing result</returns>
        Task<bool> ProcessRefundAsync(int bookingId, RefundPaymentRequestDto request);

        /// <summary>
        /// Gets user's booking history
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paginated booking history</returns>
        Task<PagedResult<BookingSummaryDto>> GetUserBookingHistoryAsync(int userId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets upcoming bookings for a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of upcoming bookings</returns>
        Task<List<BookingSummaryDto>> GetUpcomingBookingsAsync(int userId);

        /// <summary>
        /// Cleans up expired reservations (called by background service)
        /// </summary>
        /// <returns>Number of reservations cleaned up</returns>
        Task<int> CleanupExpiredReservationsAsync();

        /// <summary>
        /// Synchronizes payment status with payment provider (called by background service)
        /// </summary>
        /// <returns>Number of payments synchronized</returns>
        Task<int> SynchronizePaymentStatusAsync();
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}