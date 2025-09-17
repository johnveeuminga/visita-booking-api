using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitaBookingApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Hashed password for email/password authentication
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Google/External provider ID for OIDC authentication
        /// </summary>
        public string? ExternalId { get; set; }

        /// <summary>
        /// Provider name (e.g., "Google", "Local")
        /// </summary>
        [MaxLength(50)]
        public string Provider { get; set; } = "Local";

        public bool IsEmailVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Token used for password reset flows. Stored as a URL-safe base64 string.
        /// </summary>
        public string? PasswordResetToken { get; set; }

        /// <summary>
        /// Expiry for the password reset token (UTC)
        /// </summary>
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // Navigation properties for RBAC
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public List<string> Roles => UserRoles.Select(ur => ur.Role.Name).ToList();
    }
}
