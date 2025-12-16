using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using visita_booking_api.Models.Enums;

namespace visita_booking_api.Models.Entities
{
    public class RefundRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int RequestedByUserId { get; set; }

        [Required]
        public RefundStatus Status { get; set; } = RefundStatus.Pending;

        [Required]
        public decimal RefundAmount { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal RefundPercentage { get; set; }

        [Required]
        public decimal OriginalAmount { get; set; }

        public bool IsEligible { get; set; }

        [StringLength(500)]
        public string? EligibilityReason { get; set; }

        [StringLength(1000)]
        public string? CancellationReason { get; set; }

        [StringLength(1000)]
        public string? RejectionReason { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EvaluatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public int? ProcessedByAdminId { get; set; }

        [Column(TypeName = "json")]
        public string? PolicySnapshotJson { get; set; }

        // Navigation properties
        [ForeignKey(nameof(BookingId))]
        public virtual Booking Booking { get; set; } = null!;

        [ForeignKey(nameof(RequestedByUserId))]
        public virtual VisitaBookingApi.Models.User RequestedBy { get; set; } = null!;

        [ForeignKey(nameof(ProcessedByAdminId))]
        public virtual VisitaBookingApi.Models.User? ProcessedByAdmin { get; set; }
    }
}
