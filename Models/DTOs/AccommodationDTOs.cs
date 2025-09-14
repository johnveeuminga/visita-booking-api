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
    }

    #endregion

    #region Response DTOs

    public class AccommodationResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Logo { get; set; }
        public bool IsActive { get; set; }
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
        public bool IsActive { get; set; }
        public int ActiveRoomCount { get; set; }
    }

    #endregion
}