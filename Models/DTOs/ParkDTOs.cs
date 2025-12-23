using System.ComponentModel.DataAnnotations;

namespace visita_booking_api.Models.DTOs
{
    public class ParkDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string OpeningHours { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool HasParking { get; set; }
        public int? ParkingSlots { get; set; }
        public decimal? ParkingFee { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateParkDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string OpeningHours { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool HasParking { get; set; }

        public int? ParkingSlots { get; set; }

        public decimal? ParkingFee { get; set; }
    }

    public class UpdateParkDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string OpeningHours { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool HasParking { get; set; }

        public int? ParkingSlots { get; set; }

        public decimal? ParkingFee { get; set; }
    }
}
