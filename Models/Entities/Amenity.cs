using System.ComponentModel.DataAnnotations;

namespace visita_booking_api.Models.Entities
{
    public enum AmenityCategory
    {
        Comfort = 1,
        Technology = 2,
        Bathroom = 3,
        Kitchen = 4,
        Safety = 5,
        Accessibility = 6,
        Entertainment = 7,
        Outdoor = 8,
        Business = 9,
        Wellness = 10
    }

    public class Amenity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Icon { get; set; }

        [Required]
        public AmenityCategory Category { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        // For grouping amenities (e.g., "Premium Wi-Fi" under "Wi-Fi")
        public int? ParentAmenityId { get; set; }

        // Display order within category
        public int DisplayOrder { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();

        // Self-referencing for parent-child relationships
        public virtual Amenity? ParentAmenity { get; set; }
        public virtual ICollection<Amenity> ChildAmenities { get; set; } = new List<Amenity>();

        // Computed properties
        public bool IsParentAmenity => ParentAmenityId == null;
        public bool IsChildAmenity => ParentAmenityId != null;

        public void UpdateLastModified()
        {
            LastModified = DateTime.UtcNow;
        }
    }
}