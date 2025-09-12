using System.ComponentModel.DataAnnotations;

namespace VisitaBookingApi.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        public string? PasswordHash { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        // OAuth integration
        public string? GoogleId { get; set; }
        
        // Account status
        public bool IsEmailVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        
        // Security
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        public string? EmailVerificationToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        
        // Profile
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePictureUrl { get; set; }
        
        // Full name helper
        public string FullName => $"{FirstName} {LastName}";
    }
}
