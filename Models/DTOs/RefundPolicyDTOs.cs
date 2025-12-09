using System.ComponentModel.DataAnnotations;
using visita_booking_api.Models.Enums;

namespace visita_booking_api.Models.DTOs
{
    // DTO for creating a refund policy
    public class CreateRefundPolicyRequestDTO
    {
        [Required]
        public RefundPolicyType PolicyType { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public bool AllowsCancellation { get; set; } = true;

        [Required]
        public List<RefundPolicyTierDTO> Tiers { get; set; } = new();
    }

    // DTO for updating a refund policy
    public class UpdateRefundPolicyRequestDTO
    {
        [Required]
        public RefundPolicyType PolicyType { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public bool AllowsCancellation { get; set; }

        [Required]
        public List<RefundPolicyTierDTO> Tiers { get; set; } = new();
    }

    // DTO for refund policy tier
    public class RefundPolicyTierDTO
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int MinDaysBeforeCheckIn { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal RefundPercentage { get; set; }

        public int DisplayOrder { get; set; }
    }

    // DTO for refund policy response
    public class RefundPolicyResponseDTO
    {
        public int Id { get; set; }
        public int AccommodationId { get; set; }
        public RefundPolicyType PolicyType { get; set; }
        public string PolicyTypeName { get; set; } = string.Empty;
        public bool AllowsCancellation { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<RefundPolicyTierDTO> Tiers { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }

    // DTO for predefined policy templates
    public class PredefinedPolicyDTO
    {
        public RefundPolicyType PolicyType { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool AllowsCancellation { get; set; }
        public List<RefundPolicyTierDTO> Tiers { get; set; } = new();
    }
}
