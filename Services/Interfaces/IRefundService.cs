using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Services.Interfaces
{
    public interface IRefundService
    {
        // Policy management
        Task<RefundPolicyResponseDTO?> GetPolicyByAccommodationIdAsync(int accommodationId);
        Task<RefundPolicyResponseDTO> CreatePolicyAsync(
            int accommodationId,
            CreateRefundPolicyRequestDTO request,
            int createdByUserId
        );
        Task<RefundPolicyResponseDTO> UpdatePolicyAsync(
            int accommodationId,
            UpdateRefundPolicyRequestDTO request
        );
        Task<bool> DeletePolicyAsync(int accommodationId);
        Task<List<PredefinedPolicyDTO>> GetPredefinedPoliciesAsync();

        // Refund eligibility
        Task<RefundEligibilityResponseDTO> CheckRefundEligibilityAsync(int bookingId);

        // Refund request management
        Task<RefundRequestResponseDTO> CreateRefundRequestAsync(
            int bookingId,
            CreateRefundRequestDTO request,
            int requestedByUserId
        );
        Task<RefundRequestResponseDTO> GetRefundRequestByIdAsync(int requestId);
        Task<List<RefundRequestResponseDTO>> GetRefundRequestsByBookingIdAsync(int bookingId);
        Task<List<RefundRequestResponseDTO>> GetPendingRefundRequestsAsync(
            int pageNumber = 1,
            int pageSize = 20
        );

        // Admin actions
        Task<RefundRequestResponseDTO> ProcessRefundRequestAsync(
            int requestId,
            ProcessRefundRequestDTO request,
            int adminUserId
        );
    }
}
