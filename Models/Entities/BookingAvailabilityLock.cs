using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VisitaBookingApi.Models;

namespace visita_booking_api.Models.Entities
{
    public enum LockType
    {
        Reservation = 1,    // Lock for reservation process
        Booking = 2,       // Lock for booking confirmation
        Maintenance = 3    // Lock for maintenance activities
    }

    /// <summary>
    /// Represents a temporary lock on room availability to prevent double bookings
    /// during the reservation/booking process
    /// </summary>
    public class BookingAvailabilityLock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string LockReference { get; set; } = string.Empty;

        // Foreign Keys
        public int RoomId { get; set; }
        public int? UserId { get; set; }
        public int? BookingId { get; set; }
        public int? ReservationId { get; set; }

        // Lock Details
        [Column(TypeName = "date")]
        public DateTime CheckInDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime CheckOutDate { get; set; }

        public LockType LockType { get; set; } = LockType.Reservation;

        // Timing
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        // Session Information
        [StringLength(100)]
        public string? SessionId { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        // Status
        public bool IsActive { get; set; } = true;

        // Release Information
        public DateTime? ReleasedAt { get; set; }

        [StringLength(200)]
        public string? ReleaseReason { get; set; }

        // Audit
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(BookingId))]
        public virtual Booking? Booking { get; set; }

        [ForeignKey(nameof(ReservationId))]
        public virtual BookingReservation? Reservation { get; set; }

        // Computed Properties
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt || !IsActive;

        [NotMapped]
        public bool IsValid => IsActive && DateTime.UtcNow < ExpiresAt;

        [NotMapped]
        public TimeSpan TimeUntilExpiry => ExpiresAt - DateTime.UtcNow;

        [NotMapped]
        public int NumberOfNights => (CheckOutDate - CheckInDate).Days;
    }
}