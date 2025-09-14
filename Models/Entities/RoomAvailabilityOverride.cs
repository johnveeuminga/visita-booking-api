using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace visita_booking_api.Models.Entities
{
    public class RoomAvailabilityOverride
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        public bool IsAvailable { get; set; } = true;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OverridePrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Reason for override (maintenance, special event, etc.)
        [StringLength(100)]
        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Who made the override (for audit trail)
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; } = null!;

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}