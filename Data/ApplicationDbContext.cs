using Microsoft.EntityFrameworkCore;
using VisitaBookingApi.Models;

namespace VisitaBookingApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // RBAC entities
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        // Authentication related
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(255);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(255);
                entity.Property(u => u.PasswordHash).HasMaxLength(500);
                entity.Property(u => u.ExternalId).HasMaxLength(100);
                entity.Property(u => u.Provider).HasMaxLength(50).HasDefaultValue("Local");
                entity.HasIndex(u => u.ExternalId);
            });

            // Role entity configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
                entity.Property(r => r.Description).HasMaxLength(255);
                entity.HasIndex(r => r.Name).IsUnique();
            });

            // UserRole junction table configuration
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed default roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Guest", Description = "Regular guest users", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { Id = 2, Name = "Hotel", Description = "Hotel owners/managers", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { Id = 3, Name = "Admin", Description = "System administrators", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Seed default users
            // Note: Using the same password hash for all users
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    Email = "admin@visita.ph", 
                    FirstName = "System", 
                    LastName = "Administrator",
                    PasswordHash = "$2a$11$sXq9mWnUN0Gy2UtgD6QZ3eYfngx161BKuQlI3IOV0aQmu34NJDeBq",
                    Provider = "Local",
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User 
                { 
                    Id = 2, 
                    Email = "hotel@example.com", 
                    FirstName = "Jane", 
                    LastName = "Smith",
                    PasswordHash = "$2a$11$sXq9mWnUN0Gy2UtgD6QZ3eYfngx161BKuQlI3IOV0aQmu34NJDeBq",
                    Provider = "Local",
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User 
                { 
                    Id = 3, 
                    Email = "guest@example.com", 
                    FirstName = "John", 
                    LastName = "Doe",
                    PasswordHash = "$2a$11$sXq9mWnUN0Gy2UtgD6QZ3eYfngx161BKuQlI3IOV0aQmu34NJDeBq",
                    Provider = "Local",
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed user-role assignments
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 3, AssignedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }, // Admin user -> Admin role
                new UserRole { UserId = 2, RoleId = 2, AssignedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }, // Hotel user -> Hotel role
                new UserRole { UserId = 3, RoleId = 1, AssignedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }  // Guest user -> Guest role
            );

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
                entity.HasIndex(rt => rt.Token);
                
                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserSession configuration
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(us => us.Id);
                entity.Property(us => us.SessionToken).IsRequired().HasMaxLength(500);
                entity.Property(us => us.IpAddress).HasMaxLength(45);
                entity.Property(us => us.UserAgent).HasMaxLength(500);
                entity.HasIndex(us => us.SessionToken);
                
                entity.HasOne(us => us.User)
                    .WithMany()
                    .HasForeignKey(us => us.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}
