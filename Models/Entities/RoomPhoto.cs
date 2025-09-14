using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace visita_booking_api.Models.Entities
{
    public class RoomPhoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        [StringLength(500)]
        public string S3Key { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string S3Url { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? CdnUrl { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        // Alt text for accessibility
        [StringLength(255)]
        public string? AltText { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; } = null!;

        public void UpdateLastModified()
        {
            LastModified = DateTime.UtcNow;
        }
    }
}