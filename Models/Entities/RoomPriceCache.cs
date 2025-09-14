using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace visita_booking_api.Models.Entities
{
    /// <summary>
    /// Cached price range data for efficient room searching
    /// Updated when pricing rules change or via background job
    /// </summary>
    public class RoomPriceCache
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int RoomId { get; set; }
        
        [ForeignKey(nameof(RoomId))]
        public Room Room { get; set; } = null!;

        // Price ranges for different time periods
        [Column(TypeName = "decimal(10,2)")]
        public decimal MinPrice30Days { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MaxPrice30Days { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal AvgPrice30Days { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MinPrice90Days { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MaxPrice90Days { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal AvgPrice90Days { get; set; }

        // Seasonal multipliers for quick estimation
        [Column(TypeName = "decimal(5,2)")]
        public decimal WeekendMultiplier { get; set; } = 1.0m;
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal HolidayMultiplier { get; set; } = 1.0m;
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal PeakSeasonMultiplier { get; set; } = 1.0m;

        // Price band classification for rough filtering
        public PriceBand PriceBand { get; set; }
        
        // Cache metadata
        public DateTime LastUpdated { get; set; }
        public DateTime LastPricingRuleChange { get; set; }
        public DateTime DataValidUntil { get; set; } // When this cache expires
        
        // Performance tracking
        public int SearchHitCount { get; set; } // How often this room appears in searches
        public DateTime LastSearched { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum PriceBand
    {
        Budget,     // Bottom 25%
        Economy,    // 25-50%
        Standard,   // 50-75%
        Premium,    // 75-90%
        Luxury      // Top 10%
    }
}