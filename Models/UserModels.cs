using System.ComponentModel.DataAnnotations;

namespace VisitaBookingApi.Models
{
    // Enum for user types
    public enum UserType
    {
        User = 1,
        Hotel = 2,
        Admin = 3
    }

    // Base user entity
    public class AppUser
    {
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        public string? PhoneNumber { get; set; }
        
        [Required]
        public UserType UserType { get; set; }
        
        public string? PasswordHash { get; set; }
        
        public string? GoogleId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public bool EmailVerified { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties for specific user types
        public UserProfile? UserProfile { get; set; }
        public HotelProfile? HotelProfile { get; set; }
        public AdminProfile? AdminProfile { get; set; }
    }

    // User-specific profile
    public class UserProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;
        
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PreferredLanguage { get; set; } = "en";
    }

    // Hotel-specific profile
    public class HotelProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;
        
        [Required]
        public string HotelName { get; set; } = string.Empty;
        
        public string? BusinessRegistrationNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
    }

    // Admin-specific profile
    public class AdminProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;
        
        public string? Department { get; set; }
        public string? Role { get; set; }
        public List<string> Permissions { get; set; } = new();
        
        public DateTime? LastLoginAt { get; set; }
    }
}
