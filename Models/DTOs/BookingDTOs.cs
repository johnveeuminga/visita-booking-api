using System.ComponentModel.DataAnnotations;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Models.DTOs
{
    #region Booking Request DTOs

    public class CreateBookingRequestDto
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        public int NumberOfGuests { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string GuestEmail { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? GuestPhone { get; set; }

        [StringLength(1000)]
        public string? SpecialRequests { get; set; }

        // Reservation timeout in minutes (optional, uses default if not provided)
        [Range(5, 60)]
        public int? ReservationTimeoutMinutes { get; set; }
    }

    public class BookingAvailabilityRequestDto
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 9999)]
        public int NumberOfGuests { get; set; }
    }

    public class ExtendReservationRequestDto
    {
        [Required]
        [StringLength(20)]
        public string ReservationReference { get; set; } = string.Empty;

        [Range(5, 30, ErrorMessage = "Extension time must be between 5 and 30 minutes")]
        public int ExtensionMinutes { get; set; } = 15;
    }

    #endregion

    #region Booking Response DTOs

    public class BookingResponseDto
    {
        public int Id { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public int NumberOfNights { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string? GuestPhone { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? ActualCheckInAt { get; set; }
        public DateTime? ActualCheckOutAt { get; set; }
        
        // Reservation details
        public BookingReservationDto? Reservation { get; set; }
        
        // Payment details
        public List<BookingPaymentDto> Payments { get; set; } = new();
        
        // Room details
        public RoomSummaryDto? Room { get; set; }
        
        // Accommodation details
        public AccommodationSummaryDto? Accommodation { get; set; }
        
        // Additional fees breakdown
        public List<BookingFeeDto> AdditionalFees { get; set; } = new();
    }

    public class BookingReservationDto
    {
        public int Id { get; set; }
        public string ReservationReference { get; set; } = string.Empty;
        public ReservationStatus Status { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? PaymentUrl { get; set; }
        public DateTime? PaymentUrlExpiresAt { get; set; }
        public int ExtensionCount { get; set; }
        public bool CanExtend { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        public TimeSpan TimeUntilExpiry { get; set; }
    }

    public class BookingPaymentDto
    {
        public int Id { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public PaymentType PaymentType { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal NetAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string? FailureReason { get; set; }
        public string? CardLastFour { get; set; }
        public string? BankCode { get; set; }
        public string PaymentDescription { get; set; } = string.Empty;
    }

    public class BookingAvailabilityResponseDto
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal? EstimatedPrice { get; set; }
        public List<DateTime>? UnavailableDates { get; set; }
        public BookingPricingDetailsDto? PricingDetails { get; set; }
    }

    public class BookingPricingDetailsDto
    {
        public decimal BasePrice { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal ServiceFeeRate { get; set; }
        public decimal TotalAmount { get; set; }
        public int NumberOfNights { get; set; }
        public List<DailyPriceDto> DailyPrices { get; set; } = new();
        public List<string> AppliedPromotions { get; set; } = new();
    }

    public class DailyPriceDto
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public bool IsWeekend { get; set; }
        public bool IsHoliday { get; set; }
        public List<string> PriceModifiers { get; set; } = new();
    }

    #endregion

    #region Booking Management DTOs

    public class BookingSearchRequestDto
    {
        [StringLength(20)]
        public string? BookingReference { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? GuestEmail { get; set; }

        public BookingStatus? Status { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? CheckInDateFrom { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? CheckInDateTo { get; set; }
        
        public int? RoomId { get; set; }
        public int? UserId { get; set; }

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortDirection { get; set; } = "DESC";
    }

    public class UpdateBookingStatusDto
    {
        [Required]
        public BookingStatus Status { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }
    }

    public class CancelBookingRequestDto
    {
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string CancellationReason { get; set; } = string.Empty;

        public bool RequestRefund { get; set; } = false;

        [StringLength(100)]
        public string? CancelledBy { get; set; }
    }

    #endregion

    #region Payment DTOs

    public class ProcessPaymentRequestDto
    {
        [Required]
        [StringLength(20)]
        public string BookingReference { get; set; } = string.Empty;

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;

        [StringLength(500)]
        public string? Notes { get; set; }

        // For manual payments (admin use)
        public bool IsManualPayment { get; set; } = false;

        [StringLength(100)]
        public string? ProcessedBy { get; set; }
    }

    public class RefundPaymentRequestDto
    {
        [Required]
        [StringLength(20)]
        public string BookingReference { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal? RefundAmount { get; set; } // If null, full refund

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string RefundReason { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ProcessedBy { get; set; }
    }

    #endregion

    #region Webhook DTOs

    public class XenditWebhookDto
    {
        public string Id { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentChannel { get; set; }
        public string? BankCode { get; set; }
        public decimal? PaidAmount { get; set; }
        public string? Description { get; set; }
        public DateTime Updated { get; set; }
        public DateTime Created { get; set; }
        
        // Additional webhook fields
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    #endregion

    #region Summary DTOs

    public class RoomSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
        public int MaxGuests { get; set; }
        public string? MainPhotoUrl { get; set; }
        public List<string> Amenities { get; set; } = new();
    }

    public class BookingSummaryDto
    {
        public int Id { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfNights { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public string StatusDescription { get; set; } = string.Empty;
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusDescription { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        // Accommodation info
        public AccommodationSummaryDto? Accommodation { get; set; }
    }

    public class BookingFeeDto
    {
        public string FeeType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsPercentage { get; set; }
        public decimal? PercentageRate { get; set; }
    }

    #endregion

    #region Accommodation Booking DTOs

    public class AccommodationBookingSearchDto
    {
        public List<int> RoomIds { get; set; } = new();
        public BookingStatus? Status { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public DateTime? CheckInDateFrom { get; set; }
        public DateTime? CheckInDateTo { get; set; }
        
        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;
    }

    public class AccommodationBookingDto
    {
        public int Id { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public int NumberOfNights { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string? GuestPhone { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ActualCheckInAt { get; set; }
        public DateTime? ActualCheckOutAt { get; set; }
        // Accommodation summary (optional)
        public AccommodationSummaryDto? Accommodation { get; set; }
    }

    public class AccommodationCheckInDto
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string? GuestPhone { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public int NumberOfNights { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public BookingStatus Status { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTime? ActualCheckInAt { get; set; }
    }

    #endregion
}