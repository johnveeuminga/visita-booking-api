using System.ComponentModel.DataAnnotations;

namespace VisitaBookingApi.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        public int UserId { get; set; }

        public DateTime ExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRevoked { get; set; } = false;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }

    public class UserSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string SessionToken { get; set; } = string.Empty;

        public int UserId { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
