using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using visita_booking_api.Models.Enums;

namespace visita_booking_api.Models.Entities
{
    public class Accommodation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "longtext")]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Logo { get; set; }

        // Contact Information
        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [StringLength(20)]
        public string? ContactNo { get; set; }

        // Check-in/Check-out Times
        [StringLength(5)]
        public string? CheckInTime { get; set; }

        [StringLength(5)]
        public string? CheckOutTime { get; set; }

        // Status and approval
        public AccommodationStatus Status { get; set; } = AccommodationStatus.Pending;
        public bool IsActive { get; set; } = false; // Only active when approved
        public string? RejectionReason { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedById { get; set; }

        // Business Documents (private S3 keys)
        [StringLength(500)]
        public string? BusinessPermitS3Key { get; set; }

        [StringLength(100)]
        public string? BusinessPermitNumber { get; set; }

        [StringLength(500)]
        public string? DotAccreditationS3Key { get; set; }

        [StringLength(100)]
        public string? DotAccreditationNumber { get; set; }

        public bool IsBtcMember { get; set; } = false;

        // Additional business information
        [StringLength(500)]
        public string? OtherDocumentsS3Key { get; set; }

        [StringLength(1000)]
        public string? BusinessNotes { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Owner information (foreign key to User)
        public int? OwnerId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(OwnerId))]
        public virtual VisitaBookingApi.Models.User? Owner { get; set; }

        [ForeignKey(nameof(ApprovedById))]
        public virtual VisitaBookingApi.Models.User? ApprovedBy { get; set; }

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<AccommodationComment> Comments { get; set; } =
            new List<AccommodationComment>();

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Approve the accommodation
        /// </summary>
        public void Approve(int approvedById)
        {
            Status = AccommodationStatus.Approved;
            IsActive = true;
            ApprovedAt = DateTime.UtcNow;
            ApprovedById = approvedById;
            RejectionReason = null;
            UpdateTimestamp();
        }

        /// <summary>
        /// Reject the accommodation with a reason
        /// </summary>
        public void Reject(string reason)
        {
            Status = AccommodationStatus.Rejected;
            IsActive = false;
            RejectionReason = reason;
            ApprovedAt = null;
            ApprovedById = null;
            UpdateTimestamp();
        }

        /// <summary>
        /// Suspend the accommodation
        /// </summary>
        public void Suspend(string reason)
        {
            Status = AccommodationStatus.Suspended;
            IsActive = false;
            RejectionReason = reason;
            UpdateTimestamp();
        }
    }
}
