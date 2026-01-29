using System.ComponentModel.DataAnnotations;
using visita_booking_api.Models.Enums;
using VisitaBookingApi.Models;

namespace visita_booking_api.Models.Entities
{
    public class Establishment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public EstablishmentCategory Category { get; set; }

        public string? Logo { get; set; }
        public string? CoverImage { get; set; }

        // Location
        [Required]
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = "Baguio";
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // Contact
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? FacebookPage { get; set; }

        // Documents
        public string? BusinessPermitS3Key { get; set; }

        // Ownership & Status
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public EstablishmentStatus Status { get; set; } = EstablishmentStatus.Pending;
        public bool IsActive { get; set; } = true;

        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedById { get; set; }
        public User? ApprovedBy { get; set; }
        public string? RejectionReason { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<EstablishmentComment> Comments { get; set; } =
            new List<EstablishmentComment>();
        public ICollection<EstablishmentHours>? Hours { get; set; }
        public virtual ICollection<EstablishmentImage> Images { get; set; } =
            new List<EstablishmentImage>();
        public virtual ICollection<EstablishmentMenuItem> MenuItems { get; set; } =
            new List<EstablishmentMenuItem>();
        public virtual ICollection<EstablishmentSubcategory> Subcategories { get; set; } =
            new List<EstablishmentSubcategory>();

        // Methods (similar to Accommodation)
        public void Approve(int adminId)
        {
            Status = EstablishmentStatus.Approved;
            ApprovedAt = DateTime.UtcNow;
            ApprovedById = adminId;
            RejectionReason = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(string reason)
        {
            Status = EstablishmentStatus.Rejected;
            RejectionReason = reason;
            ApprovedAt = null;
            ApprovedById = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
