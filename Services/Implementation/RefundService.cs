using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;
using visita_booking_api.Models.Enums;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;

namespace visita_booking_api.Services.Implementation
{
    public class RefundService : IRefundService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RefundService> _logger;

        public RefundService(ApplicationDbContext context, ILogger<RefundService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Policy Management

        public async Task<RefundPolicyResponseDTO?> GetPolicyByAccommodationIdAsync(
            int accommodationId
        )
        {
            var policy = await _context
                .RefundPolicies.Include(rp => rp.Tiers.OrderBy(t => t.DisplayOrder))
                .Where(rp => rp.AccommodationId == accommodationId && rp.IsActive)
                .FirstOrDefaultAsync();

            if (policy == null)
                return null;

            return MapToResponseDTO(policy);
        }

        public async Task<RefundPolicyResponseDTO> CreatePolicyAsync(
            int accommodationId,
            CreateRefundPolicyRequestDTO request,
            int createdByUserId
        )
        {
            // Check if accommodation exists
            var accommodationExists = await _context.Accommodations.AnyAsync(a =>
                a.Id == accommodationId
            );

            if (!accommodationExists)
                throw new ArgumentException("Accommodation not found");

            // Deactivate existing policies for this accommodation
            var existingPolicies = await _context
                .RefundPolicies.Where(rp => rp.AccommodationId == accommodationId && rp.IsActive)
                .ToListAsync();

            foreach (var existingPolicy in existingPolicies)
            {
                existingPolicy.IsActive = false;
                existingPolicy.UpdateTimestamp();
            }

            // Create new policy
            var policy = new RefundPolicy
            {
                AccommodationId = accommodationId,
                PolicyType = request.PolicyType,
                Description = request.Description,
                AllowsCancellation = request.AllowsCancellation,
                IsActive = true,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.RefundPolicies.Add(policy);
            await _context.SaveChangesAsync();

            // Add tiers
            foreach (var tierDto in request.Tiers.OrderBy(t => t.DisplayOrder))
            {
                var tier = new RefundPolicyTier
                {
                    RefundPolicyId = policy.Id,
                    MinDaysBeforeCheckIn = tierDto.MinDaysBeforeCheckIn,
                    RefundPercentage = tierDto.RefundPercentage,
                    DisplayOrder = tierDto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.RefundPolicyTiers.Add(tier);
            }

            await _context.SaveChangesAsync();

            // Reload with tiers
            await _context.Entry(policy).Collection(p => p.Tiers).LoadAsync();

            return MapToResponseDTO(policy);
        }

        public async Task<RefundPolicyResponseDTO> UpdatePolicyAsync(
            int accommodationId,
            UpdateRefundPolicyRequestDTO request
        )
        {
            var policy = await _context
                .RefundPolicies.Include(rp => rp.Tiers)
                .Where(rp => rp.AccommodationId == accommodationId && rp.IsActive)
                .FirstOrDefaultAsync();

            if (policy == null)
                throw new ArgumentException("No active policy found for this accommodation");

            // Update policy
            policy.PolicyType = request.PolicyType;
            policy.Description = request.Description;
            policy.AllowsCancellation = request.AllowsCancellation;
            policy.UpdateTimestamp();

            // Remove old tiers
            _context.RefundPolicyTiers.RemoveRange(policy.Tiers);

            // Add new tiers
            foreach (var tierDto in request.Tiers.OrderBy(t => t.DisplayOrder))
            {
                var tier = new RefundPolicyTier
                {
                    RefundPolicyId = policy.Id,
                    MinDaysBeforeCheckIn = tierDto.MinDaysBeforeCheckIn,
                    RefundPercentage = tierDto.RefundPercentage,
                    DisplayOrder = tierDto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.RefundPolicyTiers.Add(tier);
            }

            await _context.SaveChangesAsync();

            // Reload with new tiers
            await _context
                .Entry(policy)
                .Collection(p => p.Tiers)
                .Query()
                .OrderBy(t => t.DisplayOrder)
                .LoadAsync();

            return MapToResponseDTO(policy);
        }

        public async Task<bool> DeletePolicyAsync(int accommodationId)
        {
            var policy = await _context
                .RefundPolicies.Where(rp => rp.AccommodationId == accommodationId && rp.IsActive)
                .FirstOrDefaultAsync();

            if (policy == null)
                return false;

            policy.IsActive = false;
            policy.UpdateTimestamp();

            await _context.SaveChangesAsync();
            return true;
        }

        public Task<List<PredefinedPolicyDTO>> GetPredefinedPoliciesAsync()
        {
            var policies = new List<PredefinedPolicyDTO>
            {
                new PredefinedPolicyDTO
                {
                    PolicyType = RefundPolicyType.Flexible,
                    Name = "Flexible",
                    Description = "Free cancellation up to 24 hours before check-in",
                    AllowsCancellation = true,
                    Tiers = new List<RefundPolicyTierDTO>
                    {
                        new RefundPolicyTierDTO
                        {
                            MinDaysBeforeCheckIn = 1,
                            RefundPercentage = 100,
                            DisplayOrder = 1,
                        },
                    },
                },
                new PredefinedPolicyDTO
                {
                    PolicyType = RefundPolicyType.Moderate,
                    Name = "Moderate",
                    Description = "50% refund if cancelled 5 or more days before check-in",
                    AllowsCancellation = true,
                    Tiers = new List<RefundPolicyTierDTO>
                    {
                        new RefundPolicyTierDTO
                        {
                            MinDaysBeforeCheckIn = 5,
                            RefundPercentage = 50,
                            DisplayOrder = 1,
                        },
                    },
                },
                new PredefinedPolicyDTO
                {
                    PolicyType = RefundPolicyType.Strict,
                    Name = "Strict",
                    Description = "No refunds within 7 days of check-in",
                    AllowsCancellation = true,
                    Tiers = new List<RefundPolicyTierDTO>
                    {
                        new RefundPolicyTierDTO
                        {
                            MinDaysBeforeCheckIn = 7,
                            RefundPercentage = 0,
                            DisplayOrder = 1,
                        },
                    },
                },
                new PredefinedPolicyDTO
                {
                    PolicyType = RefundPolicyType.NonRefundable,
                    Name = "Non-Refundable",
                    Description = "No cancellations or refunds allowed",
                    AllowsCancellation = false,
                    Tiers = new List<RefundPolicyTierDTO>(),
                },
            };

            return Task.FromResult(policies);
        }

        #endregion

        #region Refund Eligibility

        public async Task<RefundEligibilityResponseDTO> CheckRefundEligibilityAsync(int bookingId)
        {
            // Get booking with room and accommodation
            var booking = await _context
                .Bookings.Include(b => b.Room)
                .ThenInclude(r => r.Accommodation)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new ArgumentException("Booking not found");

            if (booking.Room?.Accommodation == null)
                throw new InvalidOperationException(
                    "Booking does not have associated accommodation"
                );

            // Get active refund policy
            var policy = await _context
                .RefundPolicies.Include(rp => rp.Tiers.OrderBy(t => t.MinDaysBeforeCheckIn))
                .Where(rp => rp.AccommodationId == booking.Room.AccommodationId && rp.IsActive)
                .FirstOrDefaultAsync();

            if (policy == null)
            {
                return new RefundEligibilityResponseDTO
                {
                    IsEligible = false,
                    RefundAmount = 0,
                    RefundPercentage = 0,
                    EligibilityReason = "No refund policy exists for this accommodation",
                    DaysUntilCheckIn = (booking.CheckInDate - DateTime.UtcNow.Date).Days,
                    Policy = null,
                };
            }

            // Check if cancellation is allowed
            if (!policy.AllowsCancellation)
            {
                return new RefundEligibilityResponseDTO
                {
                    IsEligible = false,
                    RefundAmount = 0,
                    RefundPercentage = 0,
                    EligibilityReason = "This booking does not allow cancellations",
                    DaysUntilCheckIn = (booking.CheckInDate - DateTime.UtcNow.Date).Days,
                    Policy = MapToResponseDTO(policy),
                };
            }

            // Calculate days until check-in
            var daysUntilCheckIn = (booking.CheckInDate - DateTime.UtcNow.Date).Days;

            // Check if already checked in
            if (daysUntilCheckIn < 0)
            {
                return new RefundEligibilityResponseDTO
                {
                    IsEligible = false,
                    RefundAmount = 0,
                    RefundPercentage = 0,
                    EligibilityReason = "Cannot cancel after check-in date has passed",
                    DaysUntilCheckIn = daysUntilCheckIn,
                    Policy = MapToResponseDTO(policy),
                };
            }

            // Find applicable tier (highest percentage for days remaining)
            var applicableTier = policy
                .Tiers.Where(t => daysUntilCheckIn >= t.MinDaysBeforeCheckIn)
                .OrderByDescending(t => t.RefundPercentage)
                .FirstOrDefault();

            if (applicableTier == null)
            {
                return new RefundEligibilityResponseDTO
                {
                    IsEligible = false,
                    RefundAmount = 0,
                    RefundPercentage = 0,
                    EligibilityReason =
                        $"Cancellation deadline has passed. Must cancel at least {policy.Tiers.Min(t => t.MinDaysBeforeCheckIn)} days before check-in",
                    DaysUntilCheckIn = daysUntilCheckIn,
                    Policy = MapToResponseDTO(policy),
                };
            }

            // Calculate refund amount
            var refundPercentage = applicableTier.RefundPercentage;
            var refundAmount = booking.TotalAmount * (refundPercentage / 100);

            return new RefundEligibilityResponseDTO
            {
                IsEligible = refundPercentage > 0,
                RefundAmount = refundAmount,
                RefundPercentage = refundPercentage,
                EligibilityReason =
                    refundPercentage > 0
                        ? $"Eligible for {refundPercentage}% refund ({daysUntilCheckIn} days before check-in)"
                        : "Not eligible for refund based on cancellation policy",
                DaysUntilCheckIn = daysUntilCheckIn,
                Policy = MapToResponseDTO(policy),
            };
        }

        #endregion

        #region Refund Request Management

        public async Task<RefundRequestResponseDTO> CreateRefundRequestAsync(
            int bookingId,
            CreateRefundRequestDTO request,
            int requestedByUserId
        )
        {
            // Check for existing refund request
            var existingRequest = await _context.RefundRequests.AnyAsync(rr =>
                rr.BookingId == bookingId
            );

            if (existingRequest)
                throw new InvalidOperationException(
                    "A refund request already exists for this booking"
                );

            // Check eligibility
            var eligibility = await CheckRefundEligibilityAsync(bookingId);

            // Get booking details
            var booking = await _context
                .Bookings.Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new ArgumentException("Booking not found");

            // Get policy snapshot
            var policy = await _context
                .RefundPolicies.Include(rp => rp.Tiers)
                .Where(rp => rp.AccommodationId == booking.Room.AccommodationId && rp.IsActive)
                .FirstOrDefaultAsync();

            var policySnapshot =
                policy != null ? JsonSerializer.Serialize(MapToResponseDTO(policy)) : null;

            // Create refund request
            var refundRequest = new RefundRequest
            {
                BookingId = bookingId,
                RequestedByUserId = requestedByUserId,
                Status = eligibility.IsEligible ? RefundStatus.AutoApproved : RefundStatus.Pending,
                RefundAmount = eligibility.RefundAmount,
                RefundPercentage = eligibility.RefundPercentage,
                OriginalAmount = booking.TotalAmount,
                IsEligible = eligibility.IsEligible,
                EligibilityReason = eligibility.EligibilityReason,
                CancellationReason = request.CancellationReason,
                RequestedAt = DateTime.UtcNow,
                EvaluatedAt = DateTime.UtcNow,
                PolicySnapshotJson = policySnapshot,
            };

            _context.RefundRequests.Add(refundRequest);
            await _context.SaveChangesAsync();

            return await GetRefundRequestByIdAsync(refundRequest.Id);
        }

        public async Task<RefundRequestResponseDTO> GetRefundRequestByIdAsync(int requestId)
        {
            var request = await _context
                .RefundRequests.Include(rr => rr.Booking)
                .Include(rr => rr.RequestedBy)
                .Include(rr => rr.ProcessedByAdmin)
                .FirstOrDefaultAsync(rr => rr.Id == requestId);

            if (request == null)
                throw new ArgumentException("Refund request not found");

            return MapToRefundRequestResponseDTO(request);
        }

        public async Task<List<RefundRequestResponseDTO>> GetRefundRequestsByBookingIdAsync(
            int bookingId
        )
        {
            var requests = await _context
                .RefundRequests.Include(rr => rr.Booking)
                .Include(rr => rr.RequestedBy)
                .Include(rr => rr.ProcessedByAdmin)
                .Where(rr => rr.BookingId == bookingId)
                .OrderByDescending(rr => rr.RequestedAt)
                .ToListAsync();

            return requests.Select(MapToRefundRequestResponseDTO).ToList();
        }

        public async Task<List<RefundRequestResponseDTO>> GetPendingRefundRequestsAsync(
            int pageNumber = 1,
            int pageSize = 20
        )
        {
            var requests = await _context
                .RefundRequests.Include(rr => rr.Booking)
                .Include(rr => rr.RequestedBy)
                .Where(rr => rr.Status == RefundStatus.Pending)
                .OrderBy(rr => rr.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return requests.Select(MapToRefundRequestResponseDTO).ToList();
        }

        #endregion

        #region Admin Actions

        public async Task<RefundRequestResponseDTO> ProcessRefundRequestAsync(
            int requestId,
            ProcessRefundRequestDTO request,
            int adminUserId
        )
        {
            var refundRequest = await _context
                .RefundRequests.Include(rr => rr.Booking)
                .Include(rr => rr.RequestedBy)
                .FirstOrDefaultAsync(rr => rr.Id == requestId);

            if (refundRequest == null)
                throw new ArgumentException("Refund request not found");

            if (refundRequest.Status != RefundStatus.Pending)
                throw new InvalidOperationException(
                    "Only pending refund requests can be processed"
                );

            // Update status
            refundRequest.Status = request.Approve ? RefundStatus.Approved : RefundStatus.Rejected;
            refundRequest.ProcessedAt = DateTime.UtcNow;
            refundRequest.ProcessedByAdminId = adminUserId;

            if (!request.Approve && !string.IsNullOrEmpty(request.RejectionReason))
            {
                refundRequest.RejectionReason = request.RejectionReason;
            }

            await _context.SaveChangesAsync();

            return await GetRefundRequestByIdAsync(requestId);
        }

        #endregion

        #region Helper Methods

        private RefundPolicyResponseDTO MapToResponseDTO(RefundPolicy policy)
        {
            return new RefundPolicyResponseDTO
            {
                Id = policy.Id,
                AccommodationId = policy.AccommodationId,
                PolicyType = policy.PolicyType,
                PolicyTypeName = policy.PolicyType.ToString(),
                AllowsCancellation = policy.AllowsCancellation,
                Description = policy.Description,
                IsActive = policy.IsActive,
                Tiers = policy
                    .Tiers.Select(t => new RefundPolicyTierDTO
                    {
                        MinDaysBeforeCheckIn = t.MinDaysBeforeCheckIn,
                        RefundPercentage = t.RefundPercentage,
                        DisplayOrder = t.DisplayOrder,
                    })
                    .ToList(),
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt,
                CreatedBy = policy.CreatedBy,
            };
        }

        private RefundRequestResponseDTO MapToRefundRequestResponseDTO(RefundRequest request)
        {
            return new RefundRequestResponseDTO
            {
                Id = request.Id,
                BookingId = request.BookingId,
                BookingReference = request.Booking?.BookingReference ?? string.Empty,
                RequestedByUserId = request.RequestedByUserId,
                RequestedByName =
                    $"{request.RequestedBy?.FirstName} {request.RequestedBy?.LastName}".Trim(),
                Status = request.Status,
                StatusName = request.Status.ToString(),
                RefundAmount = request.RefundAmount,
                RefundPercentage = request.RefundPercentage,
                OriginalAmount = request.OriginalAmount,
                IsEligible = request.IsEligible,
                EligibilityReason = request.EligibilityReason,
                CancellationReason = request.CancellationReason,
                RejectionReason = request.RejectionReason,
                RequestedAt = request.RequestedAt,
                EvaluatedAt = request.EvaluatedAt,
                ProcessedAt = request.ProcessedAt,
                ProcessedByAdminId = request.ProcessedByAdminId,
                ProcessedByAdminName =
                    request.ProcessedByAdmin != null
                        ? $"{request.ProcessedByAdmin.FirstName} {request.ProcessedByAdmin.LastName}".Trim()
                        : null,
            };
        }

        #endregion
    }
}
