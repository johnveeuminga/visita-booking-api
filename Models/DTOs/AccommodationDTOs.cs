using System.ComponentModel.DataAnnotations;

namespace visita_booking_api.Models.DTOs
{
    #region Request DTOs

    public class CreateAccommodationRequestDto
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public IFormFile? LogoFile { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [StringLength(20)]
        public string? ContactNo { get; set; }

        [StringLength(5)]
        public string? CheckInTime { get; set; }

        [StringLength(5)]
        public string? CheckOutTime { get; set; }
    }

    public class UpdateAccommodationRequestDto
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public IFormFile? LogoFile { get; set; }

        [StringLength(500)]
        public string? Logo { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [StringLength(20)]
        public string? ContactNo { get; set; }

        [StringLength(5)]
        public string? CheckInTime { get; set; }

        [StringLength(5)]
        public string? CheckOutTime { get; set; }

        // BTC membership is a boolean flag; only admins may change this via the update endpoint
        public bool? IsBtcMember { get; set; }
    }

    #endregion

    #region Response DTOs

    public class AccommodationResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Logo { get; set; }
        public string? Address { get; set; }
        public string? EmailAddress { get; set; }
        public string? ContactNo { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
        public bool IsActive { get; set; }

        // Approval/status fields
        public string Status { get; set; } = "Pending";
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedById { get; set; }
        public string? RejectionReason { get; set; }

        // Business document pre-signed URLs (generated per-request)
        public string? BusinessPermitUrl { get; set; }
        public string? DotAccreditationUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public int ActiveRoomCount { get; set; }
    }

    public class AccommodationSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Logo { get; set; }
        public string? Address { get; set; }
        public string? EmailAddress { get; set; }
        public string? ContactNo { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = "Pending";
        public int ActiveRoomCount { get; set; }
        public int CommentCount { get; set; }

        // Optional list of accommodation photo URLs. Currently may include Logo when no dedicated photos exist.
        public List<string> PhotoUrls { get; set; } = new();
    }

    #endregion
}
