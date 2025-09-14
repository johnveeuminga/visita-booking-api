using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace visita_booking_api.Models.Entities
{
    public class Accommodation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Logo { get; set; }

        // Status
        public bool IsActive { get; set; } = true;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Owner information (foreign key to User)
        [Required]
        public int OwnerId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(OwnerId))]
        public virtual VisitaBookingApi.Models.User Owner { get; set; } = null!;

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}