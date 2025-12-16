using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace visita_booking_api.Models.Entities
{
    public class RefundPolicyTier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RefundPolicyId { get; set; }

        [Required]
        public int MinDaysBeforeCheckIn { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal RefundPercentage { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(RefundPolicyId))]
        public virtual RefundPolicy RefundPolicy { get; set; } = null!;
    }
}
