using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VisitaBookingApi.Models;

namespace visita_booking_api.Models.Entities
{
    public enum BookingStatus
    {
        Reserved = 1,       // Initial reservation created, payment pending
        Confirmed = 2,      // Payment completed, booking confirmed
        CheckedIn = 3,      // Guest has checked in
        CheckedOut = 4,     // Guest has checked out
        Cancelled = 5,      // Booking cancelled
        NoShow = 6          // Guest didn't show up
    }

    public enum PaymentStatus
    {
        Pending = 1,        // Payment not yet made
        Processing = 2,     // Payment in progress
        Paid = 3,          // Payment completed
        Failed = 4,        // Payment failed
        Refunded = 5,      // Payment refunded
        PartialRefund = 6  // Partially refunded
    }

    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string BookingReference { get; set; } = string.Empty;

        // Foreign Keys
        public int UserId { get; set; }
        public int RoomId { get; set; }

        // Booking Details
        [Required]
        [Column(TypeName = "date")]
        public DateTime CheckInDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 20)]
        public int NumberOfGuests { get; set; }

        [Range(1, int.MaxValue)]
        public int NumberOfNights { get; set; }

        // Pricing
        [Column(TypeName = "decimal(12,2)")]
        public decimal BaseAmount { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal ServiceFee { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        // Status Management
        public BookingStatus Status { get; set; } = BookingStatus.Reserved;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        // Guest Information
        [Required]
        [StringLength(100)]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string GuestEmail { get; set; } = string.Empty;

        [StringLength(20)]
        public string? GuestPhone { get; set; }

        [StringLength(1000)]
        public string? SpecialRequests { get; set; }

        // Concurrency Control
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string? CreatedBy { get; set; }
        
        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        // Cancellation
        public DateTime? CancelledAt { get; set; }
        
        [StringLength(100)]
        public string? CancelledBy { get; set; }
        
        [StringLength(500)]
        public string? CancellationReason { get; set; }

        // Check-in/Check-out
        public DateTime? ActualCheckInAt { get; set; }
        public DateTime? ActualCheckOutAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; } = null!;

        public virtual ICollection<BookingPayment> Payments { get; set; } = new List<BookingPayment>();
        public virtual BookingReservation? Reservation { get; set; }

        // Computed Properties
        [NotMapped]
        public bool IsActive => Status != BookingStatus.Cancelled;

        [NotMapped]
        public bool IsConfirmed => Status == BookingStatus.Confirmed;

        [NotMapped]
        public bool IsPaid => PaymentStatus == PaymentStatus.Paid;

        [NotMapped]
        public TimeSpan Duration => CheckOutDate - CheckInDate;
    }
}