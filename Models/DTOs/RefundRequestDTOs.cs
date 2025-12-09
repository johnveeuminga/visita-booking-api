using System.ComponentModel.DataAnnotations;
using visita_booking_api.Models.Enums;

namespace visita_booking_api.Models.DTOs
{
    // DTO for creating a refund request
    public class CreateRefundRequestDTO
    {
        [Required]
        [StringLength(1000)]
        public string CancellationReason { get; set; } = string.Empty;
    }

    // DTO for refund eligibility check (preview before submitting)
    public class RefundEligibilityResponseDTO
    {
        public bool IsEligible { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal RefundPercentage { get; set; }
        public string EligibilityReason { get; set; } = string.Empty;
        public int DaysUntilCheckIn { get; set; }
        public RefundPolicyResponseDTO? Policy { get; set; }
    }

    // DTO for refund request response
    public class RefundRequestResponseDTO
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public int RequestedByUserId { get; set; }
        public string RequestedByName { get; set; } = string.Empty;
        public RefundStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public decimal RefundPercentage { get; set; }
        public decimal OriginalAmount { get; set; }
        public bool IsEligible { get; set; }
        public string? EligibilityReason { get; set; }
        public string? CancellationReason { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? EvaluatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public int? ProcessedByAdminId { get; set; }
        public string? ProcessedByAdminName { get; set; }
    }

    // DTO for admin to approve/reject refund
    public class ProcessRefundRequestDTO
    {
        [Required]
        public bool Approve { get; set; }

        [StringLength(1000)]
        public string? RejectionReason { get; set; }
    }
}
