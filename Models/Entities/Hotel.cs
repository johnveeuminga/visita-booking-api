using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace visita_booking_api.Models.Entities
{
    public class Hotel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        // Rating and reviews
        [Range(0, 5)]
        public decimal Rating { get; set; } = 0;

        public int ReviewCount { get; set; } = 0;

        // Business details
        [StringLength(100)]
        public string? BusinessRegistrationNumber { get; set; }

        [StringLength(100)]
        public string? TaxId { get; set; }

        // Hotel amenities/features
        public bool HasParking { get; set; } = false;
        public bool HasWifi { get; set; } = true;
        public bool HasPool { get; set; } = false;
        public bool HasGym { get; set; } = false;
        public bool HasSpa { get; set; } = false;
        public bool HasRestaurant { get; set; } = false;

        // Check-in/out times
        public TimeSpan CheckInTime { get; set; } = new TimeSpan(14, 0, 0); // 2:00 PM
        public TimeSpan CheckOutTime { get; set; } = new TimeSpan(11, 0, 0); // 11:00 AM

        // Status
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Owner information (foreign key to User)
        public int? OwnerId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(OwnerId))]
        public virtual VisitaBookingApi.Models.User? Owner { get; set; }

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        // Computed properties
        [NotMapped]
        public string FullAddress => $"{Address}, {City}, {Country}" + 
            (string.IsNullOrEmpty(PostalCode) ? "" : $" {PostalCode}");

        [NotMapped]
        public int ActiveRoomCount => Rooms?.Count(r => r.IsActive) ?? 0;
    }
}