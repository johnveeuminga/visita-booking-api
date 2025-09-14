using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace visita_booking_api.Models.Entities
{
    public class HolidayCalendar
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(5)]
        public string Country { get; set; } = "US"; // ISO country code

        public bool IsNational { get; set; } = true;

        [Column(TypeName = "decimal(5,4)")]
        public decimal PriceMultiplier { get; set; } = 1.0m;

        public bool IsActive { get; set; } = true;

        // Holiday type (religious, national, etc.)
        [StringLength(50)]
        public string? HolidayType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}