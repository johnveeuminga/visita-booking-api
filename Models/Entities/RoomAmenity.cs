using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace visita_booking_api.Models.Entities
{
    public class RoomAmenity
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public int AmenityId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Optional: If amenity has different configurations per room
        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; } = null!;

        [ForeignKey(nameof(AmenityId))]
        public virtual Amenity Amenity { get; set; } = null!;
    }
}