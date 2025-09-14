using System.ComponentModel.DataAnnotations;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Models.DTOs
{
    public class RoomDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
        public int MaxGuests { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<RoomPhotoDTO> Photos { get; set; } = new();
        public List<AmenityDTO> Amenities { get; set; } = new();
        public List<object> PricingRules { get; set; } = new();
        public string? MainPhotoUrl { get; set; }
        public AccommodationSummaryDto? Accommodation { get; set; }
    }

    public class RoomListItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
        public int MaxGuests { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? MainPhotoUrl { get; set; }
        public int PhotoCount { get; set; }
        public int AmenityCount { get; set; }
        public List<string> MainAmenities { get; set; } = new();
        public AccommodationSummaryDto? Accommodation { get; set; }
    }

    public class RoomCreateDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public decimal DefaultPrice { get; set; }
        
        [Range(1, 20)]
        public int MaxGuests { get; set; } = 2;
        
        public List<int> AmenityIds { get; set; } = new();
        
        // Photo upload support
        public List<IFormFile>? PhotoFiles { get; set; }

        [Required]
        public int AccommodationId { get; set; }
    }

    public class RoomUpdateDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public decimal DefaultPrice { get; set; }
        
        [Range(1, 20)]
        public int MaxGuests { get; set; } = 2;
        
        public bool IsActive { get; set; } = true;
        
        public List<int> AmenityIds { get; set; } = new();
        
        // Photo upload support
        public List<IFormFile>? PhotoFiles { get; set; }
        
        // Photo management - specify which existing photos to delete
        public List<int>? PhotosToDelete { get; set; }
    }

    public class RoomPhotoDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
    }

    public class RoomPhotoUploadDTO
    {
        [Required]
        public IFormFile File { get; set; } = null!;
        
        public int DisplayOrder { get; set; }
        public string? AltText { get; set; }
    }

    public class AmenityDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AmenityCategory Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastModified { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class AmenityCreateDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public AmenityCategory Category { get; set; }
    }

    public class AmenityUpdateDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public AmenityCategory Category { get; set; }
    }

    public class AmenityUsageDTO
    {
        public int AmenityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoomPricingRuleDTO
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RuleType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? DayOfWeek { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal FixedPrice { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public int? MinimumNights { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class RoomPricingRuleCreateDTO
    {
        [Required]
        public string RuleType { get; set; } = string.Empty; // Default, Weekend, Holiday, Seasonal, SpecialEvent, LongStay

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int? DayOfWeek { get; set; } // 0=Sunday, 1=Monday, etc.

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [Range(0.01, 99999.99)]
        public decimal FixedPrice { get; set; }

        [Range(0, 1000)]
        public int Priority { get; set; } = 0;

        [Range(1, 30)]
        public int? MinimumNights { get; set; }
    }
}
