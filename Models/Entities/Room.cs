using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace visita_booking_api.Models.Entities
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultPrice { get; set; }

        [Range(1, 20)]
        public int MaxGuests { get; set; } = 2;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // For multi-hotel support
        public int? HotelId { get; set; }

        // Cache versioning for invalidation
        public int CacheVersion { get; set; } = 1;

        // Navigation properties
        [ForeignKey(nameof(HotelId))]
        public virtual Hotel? Hotel { get; set; }
        public virtual ICollection<RoomPhoto> Photos { get; set; } = new List<RoomPhoto>();
        public virtual ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
        public virtual ICollection<RoomPricingRule> PricingRules { get; set; } = new List<RoomPricingRule>();
        public virtual ICollection<RoomAvailabilityOverride> AvailabilityOverrides { get; set; } = new List<RoomAvailabilityOverride>();

        // Computed properties
        [NotMapped]
        public List<Amenity> Amenities => RoomAmenities?.Select(ra => ra.Amenity).Where(a => a != null).ToList() ?? new List<Amenity>();

        [NotMapped]
        public string? MainPhotoUrl => Photos?.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).FirstOrDefault()?.CdnUrl ?? Photos?.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).FirstOrDefault()?.S3Url;

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
            CacheVersion++;
        }
    }
}