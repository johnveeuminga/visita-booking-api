using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using visita_booking_api.Models.Enums;

namespace visita_booking_api.Models.Entities
{
    public class RefundPolicy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccommodationId { get; set; }

        [Required]
        public RefundPolicyType PolicyType { get; set; }

        public bool AllowsCancellation { get; set; } = true;

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        // Navigation properties
        [ForeignKey(nameof(AccommodationId))]
        public virtual Accommodation Accommodation { get; set; } = null!;

        [ForeignKey(nameof(CreatedBy))]
        public virtual VisitaBookingApi.Models.User? Creator { get; set; }

        public virtual ICollection<RefundPolicyTier> Tiers { get; set; } =
            new List<RefundPolicyTier>();

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
