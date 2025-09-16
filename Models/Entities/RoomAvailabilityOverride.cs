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

    // Optional explicit available unit count for this date. If set, it overrides TotalUnits.
    // Note: AvailableCount may exceed Room.TotalUnits to support special offerings or external capacity on specific dates.
    // AvailableCount is authoritative for the date when present. To remove availability for a date, set AvailableCount = 0.
        [Range(0, 1000)]
        public int? AvailableCount { get; set; }

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