using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace visita_booking_api.Models.Entities
{
    public enum PricingRuleType
    {
        Default = 1,
        Weekend = 2,
        Holiday = 3,
        Seasonal = 4,
        SpecialEvent = 5,
        LongStay = 6
    }

    public class RoomPricingRule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public PricingRuleType RuleType { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Day of week (0=Sunday, 1=Monday, etc.) for Weekend rules
        public int? DayOfWeek { get; set; }

        // Date range for Seasonal or SpecialEvent rules
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Simplified pricing - just a fixed price for this rule
        [Column(TypeName = "decimal(10,2)")]
        [Required]
        public decimal FixedPrice { get; set; }

        public bool IsActive { get; set; } = true;

        // Higher priority rules override lower ones
        public int Priority { get; set; } = 0;

        // Minimum stay requirements
        public int? MinimumNights { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; } = null!;

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        // Validation methods
        public bool IsValidForDate(DateTime date)
        {
            if (!IsActive) return false;

            return RuleType switch
            {
                PricingRuleType.Weekend => DayOfWeek.HasValue && (int)date.DayOfWeek == DayOfWeek.Value,
                PricingRuleType.Seasonal or PricingRuleType.SpecialEvent => 
                    StartDate.HasValue && EndDate.HasValue && 
                    date >= StartDate.Value && date <= EndDate.Value,
                _ => true
            };
        }

        public decimal CalculatePrice(decimal basePrice)
        {
            // With simplified pricing, we just return the fixed price for this rule
            // The basePrice parameter is kept for interface compatibility but not used
            return FixedPrice;
        }
    }
}