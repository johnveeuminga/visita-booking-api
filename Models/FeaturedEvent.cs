using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitaBookingApi.Models
{
    [Table("featured_events")]
    public class FeaturedEvent
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [MaxLength(500)]
        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [MaxLength(500)]
        [Column("image_url_2")]
        public string? ImageUrl2 { get; set; }

        [MaxLength(500)]
        [Column("image_url_3")]
        public string? ImageUrl3 { get; set; }

        [MaxLength(500)]
        [Column("image_url_4")]
        public string? ImageUrl4 { get; set; }

        [MaxLength(500)]
        [Column("image_url_5")]
        public string? ImageUrl5 { get; set; }

        [MaxLength(500)]
        [Column("event_url")]
        public string? EventUrl { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class FeaturedEventDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? ImageUrl { get; set; }
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }
        public string? ImageUrl4 { get; set; }
        public string? ImageUrl5 { get; set; }

        public string? EventUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
