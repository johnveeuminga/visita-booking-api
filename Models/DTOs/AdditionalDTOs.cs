using System.ComponentModel.DataAnnotations;

namespace visita_booking_api.Models.DTOs
{
    public class AmenityBulkUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [StringLength(255)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(100)]
        public string? Icon { get; set; }

        public bool? IsActive { get; set; }
    }

    public class AmenityUsageStatsDTO
    {
        public int AmenityId { get; set; }
        public string AmenityName { get; set; } = string.Empty;
        public int TotalRoomsWithAmenity { get; set; }
        public int ActiveRoomsWithAmenity { get; set; }
        public double UsagePercentage { get; set; }
        public DateTime LastCalculated { get; set; } = DateTime.UtcNow;
    }
}