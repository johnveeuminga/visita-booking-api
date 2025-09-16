using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VisitaBookingApi.Models;

namespace visita_booking_api.Models.Entities
{
    public enum ReservationStatus
    {
        Active = 1,      // Reservation is active and room is held
        Expired = 2,     // Reservation has expired
        Confirmed = 3,   // Reservation converted to confirmed booking
        Cancelled = 4    // Reservation was cancelled before expiry
    }

    /// <summary>
    /// Represents a temporary reservation that holds a room for a specific period
    /// while the user completes the payment process
    /// </summary>
    public class BookingReservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string ReservationReference { get; set; } = string.Empty;

        // Foreign Keys
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }

        // Reservation Timing
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        // Booking Details (cached for performance)
        [Column(TypeName = "date")]
        public DateTime CheckInDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime CheckOutDate { get; set; }

        public int NumberOfGuests { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        // Status and Payment
        public ReservationStatus Status { get; set; } = ReservationStatus.Active;

        // Payment Integration (Xendit)
        [StringLength(100)]
        public string? XenditInvoiceId { get; set; }

        [StringLength(500)]
        public string? PaymentUrl { get; set; }

        public DateTime? PaymentUrlExpiresAt { get; set; }

        // Extension/Renewal
        public int ExtensionCount { get; set; } = 0;
        public DateTime? LastExtendedAt { get; set; }

        // Audit
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        // Completion/Cancellation
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        
        [StringLength(500)]
        public string? CancellationReason { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(BookingId))]
        public virtual Booking Booking { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; } = null!;

        // Computed Properties
        [NotMapped]
        public bool IsActive => Status == ReservationStatus.Active && DateTime.UtcNow < ExpiresAt;

        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt || Status == ReservationStatus.Expired;

        [NotMapped]
        public TimeSpan TimeUntilExpiry => ExpiresAt - DateTime.UtcNow;

        [NotMapped]
        public TimeSpan ReservationDuration => ExpiresAt - ReservedAt;

        [NotMapped]
        public bool CanExtend => Status == ReservationStatus.Active && ExtensionCount < 2; // Max 2 extensions
    }
}