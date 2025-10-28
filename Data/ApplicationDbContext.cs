using Microsoft.EntityFrameworkCore;
using VisitaBookingApi.Models;
using visita_booking_api.Models.Entities;

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

        
        // Accommodation management entities
        public DbSet<Accommodation> Accommodations { get; set; }
// Accommodation comment
         public DbSet<AccommodationComment> AccommodationComments { get; set; }
        
         // Room management entities
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomPhoto> RoomPhotos { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<RoomAmenity> RoomAmenities { get; set; }
        public DbSet<RoomPricingRule> RoomPricingRules { get; set; }
        public DbSet<RoomAvailabilityOverride> RoomAvailabilityOverrides { get; set; }
        public DbSet<HolidayCalendar> HolidayCalendar { get; set; }
        public DbSet<RoomPriceCache> RoomPriceCaches { get; set; }

        // Booking management entities
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingReservation> BookingReservations { get; set; }
        public DbSet<BookingPayment> BookingPayments { get; set; }
        public DbSet<BookingAvailabilityLock> BookingAvailabilityLocks { get; set; }

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

            // Accommodation entity configuration
            modelBuilder.Entity<Accommodation>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).IsRequired().HasMaxLength(200);
                entity.Property(a => a.Description).HasMaxLength(1000);
                entity.Property(a => a.Logo).HasMaxLength(500);
                entity.HasIndex(a => a.Name);
                entity.HasIndex(a => a.IsActive);
                entity.HasIndex(a => a.OwnerId);

                entity.HasOne(a => a.Owner)
                    .WithMany()
                    .HasForeignKey(a => a.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Room entity configuration
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Description).HasMaxLength(2000);
                entity.Property(r => r.DefaultPrice).HasPrecision(10, 2);
                entity.Property(r => r.MaxGuests).HasDefaultValue(2);
                entity.HasIndex(r => r.Name);
                entity.HasIndex(r => r.IsActive);
                entity.HasIndex(r => r.AccommodationId);
                entity.HasIndex(r => r.MaxGuests);

                entity.HasOne(r => r.Accommodation)
                    .WithMany(a => a.Rooms)
                    .HasForeignKey(r => r.AccommodationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // RoomPhoto entity configuration
            modelBuilder.Entity<RoomPhoto>(entity =>
            {
                entity.HasKey(rp => rp.Id);
                entity.Property(rp => rp.S3Key).IsRequired().HasMaxLength(500);
                entity.Property(rp => rp.S3Url).IsRequired().HasMaxLength(1000);
                entity.Property(rp => rp.CdnUrl).HasMaxLength(1000);
                entity.Property(rp => rp.FileName).IsRequired().HasMaxLength(255);
                entity.Property(rp => rp.ContentType).IsRequired().HasMaxLength(50);
                entity.Property(rp => rp.AltText).HasMaxLength(255);
                entity.HasIndex(rp => new { rp.RoomId, rp.DisplayOrder });
                entity.HasIndex(rp => rp.IsActive);

                entity.HasOne(rp => rp.Room)
                    .WithMany(r => r.Photos)
                    .HasForeignKey(rp => rp.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Amenity entity configuration
            modelBuilder.Entity<Amenity>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Description).HasMaxLength(500);
                entity.Property(a => a.Icon).HasMaxLength(100);
                entity.HasIndex(a => a.Category);
                entity.HasIndex(a => a.IsActive);
                entity.HasIndex(a => a.ParentAmenityId);

                entity.HasOne(a => a.ParentAmenity)
                    .WithMany(a => a.ChildAmenities)
                    .HasForeignKey(a => a.ParentAmenityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RoomAmenity entity configuration
            modelBuilder.Entity<RoomAmenity>(entity =>
            {
                entity.HasKey(ra => new { ra.RoomId, ra.AmenityId });
                entity.Property(ra => ra.Notes).HasMaxLength(500);

                entity.HasOne(ra => ra.Room)
                    .WithMany(r => r.RoomAmenities)
                    .HasForeignKey(ra => ra.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ra => ra.Amenity)
                    .WithMany(a => a.RoomAmenities)
                    .HasForeignKey(ra => ra.AmenityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RoomPricingRule entity configuration
            modelBuilder.Entity<RoomPricingRule>(entity =>
            {
                entity.HasKey(rpr => rpr.Id);
                entity.Property(rpr => rpr.Name).HasMaxLength(100);
                entity.Property(rpr => rpr.Description).HasMaxLength(500);
                entity.Property(rpr => rpr.FixedPrice).HasPrecision(10, 2);
                entity.HasIndex(rpr => new { rpr.RoomId, rpr.RuleType, rpr.IsActive });
                entity.HasIndex(rpr => new { rpr.StartDate, rpr.EndDate });

                entity.HasOne(rpr => rpr.Room)
                    .WithMany(r => r.PricingRules)
                    .HasForeignKey(rpr => rpr.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RoomAvailabilityOverride entity configuration
            modelBuilder.Entity<RoomAvailabilityOverride>(entity =>
            {
                entity.HasKey(rao => rao.Id);
                entity.Property(rao => rao.Date).HasColumnType("date");
                entity.Property(rao => rao.OverridePrice).HasPrecision(10, 2);
                entity.Property(rao => rao.Notes).HasMaxLength(500);
                entity.Property(rao => rao.Reason).HasMaxLength(100);
                entity.Property(rao => rao.CreatedBy).HasMaxLength(100);
                entity.HasIndex(rao => new { rao.RoomId, rao.Date }).IsUnique();

                entity.HasOne(rao => rao.Room)
                    .WithMany(r => r.AvailabilityOverrides)
                    .HasForeignKey(rao => rao.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // HolidayCalendar entity configuration
            modelBuilder.Entity<HolidayCalendar>(entity =>
            {
                entity.HasKey(hc => hc.Id);
                entity.Property(hc => hc.Date).HasColumnType("date");
                entity.Property(hc => hc.Name).IsRequired().HasMaxLength(100);
                entity.Property(hc => hc.Description).HasMaxLength(500);
                entity.Property(hc => hc.Country).HasMaxLength(5);
                entity.Property(hc => hc.PriceMultiplier).HasPrecision(5, 4);
                entity.Property(hc => hc.HolidayType).HasMaxLength(50);
                entity.HasIndex(hc => new { hc.Date, hc.Country });
                entity.HasIndex(hc => hc.IsActive);
            });

            // RoomPriceCache entity configuration
            modelBuilder.Entity<RoomPriceCache>(entity =>
            {
                entity.HasKey(rpc => rpc.Id);
                entity.Property(rpc => rpc.MinPrice30Days).HasPrecision(10, 2);
                entity.Property(rpc => rpc.MaxPrice30Days).HasPrecision(10, 2);
                entity.Property(rpc => rpc.AvgPrice30Days).HasPrecision(10, 2);
                entity.Property(rpc => rpc.MinPrice90Days).HasPrecision(10, 2);
                entity.Property(rpc => rpc.MaxPrice90Days).HasPrecision(10, 2);
                entity.Property(rpc => rpc.AvgPrice90Days).HasPrecision(10, 2);
                entity.Property(rpc => rpc.WeekendMultiplier).HasPrecision(5, 2).HasDefaultValue(1.0m);
                entity.Property(rpc => rpc.HolidayMultiplier).HasPrecision(5, 2).HasDefaultValue(1.0m);
                entity.Property(rpc => rpc.PeakSeasonMultiplier).HasPrecision(5, 2).HasDefaultValue(1.0m);
                
                // Indexes for fast price range filtering
                entity.HasIndex(rpc => rpc.RoomId).IsUnique();
                entity.HasIndex(rpc => new { rpc.MinPrice30Days, rpc.MaxPrice30Days });
                entity.HasIndex(rpc => new { rpc.MinPrice90Days, rpc.MaxPrice90Days });
                entity.HasIndex(rpc => rpc.PriceBand);
                entity.HasIndex(rpc => rpc.DataValidUntil);
                entity.HasIndex(rpc => rpc.LastUpdated);
                
                // Foreign key relationship
                entity.HasOne(rpc => rpc.Room)
                    .WithMany()
                    .HasForeignKey(rpc => rpc.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Booking entity configuration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.BookingReference).IsRequired().HasMaxLength(255);
                entity.Property(b => b.GuestName).IsRequired().HasMaxLength(100);
                entity.Property(b => b.GuestEmail).IsRequired().HasMaxLength(100);
                entity.Property(b => b.GuestPhone).HasMaxLength(20);
                entity.Property(b => b.SpecialRequests).HasMaxLength(1000);
                entity.Property(b => b.BaseAmount).HasPrecision(12, 2);
                entity.Property(b => b.TaxAmount).HasPrecision(12, 2);
                entity.Property(b => b.ServiceFee).HasPrecision(12, 2);
                entity.Property(b => b.TotalAmount).HasPrecision(12, 2);
                entity.Property(b => b.CheckInDate).HasColumnType("date");
                entity.Property(b => b.CheckOutDate).HasColumnType("date");
                entity.Property(b => b.CancellationReason).HasMaxLength(500);
                entity.Property(b => b.CreatedBy).HasMaxLength(100);
                entity.Property(b => b.UpdatedBy).HasMaxLength(100);
                entity.Property(b => b.CancelledBy).HasMaxLength(100);

                // Indexes
                entity.HasIndex(b => b.BookingReference).IsUnique();
                entity.HasIndex(b => b.GuestEmail);
                entity.HasIndex(b => b.Status);
                entity.HasIndex(b => b.PaymentStatus);
                entity.HasIndex(b => new { b.CheckInDate, b.CheckOutDate });
                entity.HasIndex(b => b.CreatedAt);
                entity.HasIndex(b => b.UserId);
                entity.HasIndex(b => b.RoomId);

                // Foreign key relationships
                entity.HasOne(b => b.User)
                    .WithMany()
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Room)
                    .WithMany()
                    .HasForeignKey(b => b.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BookingReservation entity configuration
            modelBuilder.Entity<BookingReservation>(entity =>
            {
                entity.HasKey(br => br.Id);
                entity.Property(br => br.ReservationReference).IsRequired().HasMaxLength(255);
                entity.Property(br => br.CheckInDate).HasColumnType("date");
                entity.Property(br => br.CheckOutDate).HasColumnType("date");
                entity.Property(br => br.TotalAmount).HasPrecision(12, 2);
                entity.Property(br => br.XenditInvoiceId).HasMaxLength(100);
                entity.Property(br => br.PaymentUrl).HasMaxLength(500);
                entity.Property(br => br.CancellationReason).HasMaxLength(500);
                entity.Property(br => br.UpdatedBy).HasMaxLength(100);

                // Indexes
                entity.HasIndex(br => br.ReservationReference).IsUnique();
                entity.HasIndex(br => br.XenditInvoiceId);
                entity.HasIndex(br => br.Status);
                entity.HasIndex(br => br.ExpiresAt);
                entity.HasIndex(br => br.ReservedAt);
                entity.HasIndex(br => new { br.RoomId, br.CheckInDate, br.CheckOutDate });
                entity.HasIndex(br => br.UserId);

                // Foreign key relationships
                entity.HasOne(br => br.Booking)
                    .WithOne(b => b.Reservation)
                    .HasForeignKey<BookingReservation>(br => br.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(br => br.User)
                    .WithMany()
                    .HasForeignKey(br => br.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(br => br.Room)
                    .WithMany()
                    .HasForeignKey(br => br.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BookingPayment entity configuration
            modelBuilder.Entity<BookingPayment>(entity =>
            {
                entity.HasKey(bp => bp.Id);
                entity.Property(bp => bp.PaymentReference).IsRequired().HasMaxLength(255);
                entity.Property(bp => bp.Amount).HasPrecision(12, 2);
                entity.Property(bp => bp.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("PHP");
                entity.Property(bp => bp.NetAmount).HasPrecision(12, 2);
                entity.Property(bp => bp.ProviderFee).HasPrecision(10, 2);
                entity.Property(bp => bp.PlatformFee).HasPrecision(10, 2);
                entity.Property(bp => bp.XenditInvoiceId).HasMaxLength(100);
                entity.Property(bp => bp.XenditPaymentId).HasMaxLength(100);
                entity.Property(bp => bp.XenditExternalId).HasMaxLength(100);
                entity.Property(bp => bp.XenditPaymentUrl).HasMaxLength(500);
                entity.Property(bp => bp.ProviderTransactionId).HasMaxLength(100);
                entity.Property(bp => bp.ProviderPaymentMethod).HasMaxLength(100);
                entity.Property(bp => bp.CardLastFour).HasMaxLength(4);
                entity.Property(bp => bp.BankCode).HasMaxLength(50);
                entity.Property(bp => bp.FailureReason).HasMaxLength(1000);
                entity.Property(bp => bp.RefundReason).HasMaxLength(500);
                entity.Property(bp => bp.CreatedBy).HasMaxLength(100);

                // JSON columns
                entity.Property(bp => bp.XenditWebhookData).HasColumnType("json");
                entity.Property(bp => bp.ProviderMetadata).HasColumnType("json");

                // Indexes
                entity.HasIndex(bp => bp.PaymentReference).IsUnique();
                entity.HasIndex(bp => bp.XenditInvoiceId);
                entity.HasIndex(bp => bp.XenditPaymentId);
                entity.HasIndex(bp => bp.Status);
                entity.HasIndex(bp => bp.PaymentType);
                entity.HasIndex(bp => bp.CreatedAt);
                entity.HasIndex(bp => bp.BookingId);

                // Foreign key relationships
                entity.HasOne(bp => bp.Booking)
                    .WithMany(b => b.Payments)
                    .HasForeignKey(bp => bp.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bp => bp.RefundedFromPayment)
                    .WithMany(p => p.RefundPayments)
                    .HasForeignKey(bp => bp.RefundedFromPaymentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BookingAvailabilityLock entity configuration
            modelBuilder.Entity<BookingAvailabilityLock>(entity =>
            {
                entity.HasKey(bal => bal.Id);
                entity.Property(bal => bal.LockReference).IsRequired().HasMaxLength(30);
                entity.Property(bal => bal.CheckInDate).HasColumnType("date");
                entity.Property(bal => bal.CheckOutDate).HasColumnType("date");
                entity.Property(bal => bal.SessionId).HasMaxLength(100);
                entity.Property(bal => bal.IpAddress).HasMaxLength(45);
                entity.Property(bal => bal.UserAgent).HasMaxLength(500);
                entity.Property(bal => bal.ReleaseReason).HasMaxLength(200);
                entity.Property(bal => bal.CreatedBy).HasMaxLength(100);

                // Indexes
                entity.HasIndex(bal => bal.LockReference).IsUnique();
                entity.HasIndex(bal => bal.RoomId);
                entity.HasIndex(bal => bal.IsActive);
                entity.HasIndex(bal => bal.ExpiresAt);
                entity.HasIndex(bal => new { bal.RoomId, bal.CheckInDate, bal.CheckOutDate });
                entity.HasIndex(bal => bal.CreatedAt);

                // Foreign key relationships
                entity.HasOne(bal => bal.Room)
                    .WithMany()
                    .HasForeignKey(bal => bal.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bal => bal.User)
                    .WithMany()
                    .HasForeignKey(bal => bal.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(bal => bal.Booking)
                    .WithMany()
                    .HasForeignKey(bal => bal.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(bal => bal.Reservation)
                    .WithMany()
                    .HasForeignKey(bal => bal.ReservationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
             // AccommodationComment entity configuration
            // Internal admin notes only - not visible to accommodation owners
            modelBuilder.Entity<AccommodationComment>(entity =>
            {
                entity.HasKey(ac => ac.Id);
                entity.Property(ac => ac.Comment).IsRequired().HasMaxLength(2000);
                entity.HasIndex(ac => ac.AccommodationId);
                entity.HasIndex(ac => ac.AdminId);
                entity.HasIndex(ac => ac.CreatedAt);

                // Foreign key relationships
                entity.HasOne(ac => ac.Accommodation)
                    .WithMany(a => a.Comments)
                    .HasForeignKey(ac => ac.AccommodationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ac => ac.Admin)
                    .WithMany()
                    .HasForeignKey(ac => ac.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        

            // Seed default amenities
            SeedAmenities(modelBuilder);
        }


           

        private static void SeedAmenities(ModelBuilder modelBuilder)
        {
            var amenities = new List<Amenity>
            {
                // Comfort
                new() { Id = 1, Name = "Air Conditioning", Category = AmenityCategory.Comfort, Icon = "ac-unit", DisplayOrder = 1, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 2, Name = "Heating", Category = AmenityCategory.Comfort, Icon = "whatshot", DisplayOrder = 2, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 3, Name = "Balcony", Category = AmenityCategory.Comfort, Icon = "deck", DisplayOrder = 3, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 4, Name = "City View", Category = AmenityCategory.Comfort, Icon = "location-city", DisplayOrder = 4, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 5, Name = "Ocean View", Category = AmenityCategory.Comfort, Icon = "waves", DisplayOrder = 5, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Technology
                new() { Id = 6, Name = "Free WiFi", Category = AmenityCategory.Technology, Icon = "wifi", DisplayOrder = 1, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 7, Name = "Smart TV", Category = AmenityCategory.Technology, Icon = "tv", DisplayOrder = 2, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 8, Name = "USB Charging Ports", Category = AmenityCategory.Technology, Icon = "usb", DisplayOrder = 3, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 9, Name = "Bluetooth Speaker", Category = AmenityCategory.Technology, Icon = "speaker", DisplayOrder = 4, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Bathroom
                new() { Id = 10, Name = "Private Bathroom", Category = AmenityCategory.Bathroom, Icon = "bathroom", DisplayOrder = 1, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 11, Name = "Shower", Category = AmenityCategory.Bathroom, Icon = "shower", DisplayOrder = 2, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 12, Name = "Bathtub", Category = AmenityCategory.Bathroom, Icon = "bathtub", DisplayOrder = 3, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 13, Name = "Hair Dryer", Category = AmenityCategory.Bathroom, Icon = "dry", DisplayOrder = 4, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 14, Name = "Toiletries", Category = AmenityCategory.Bathroom, Icon = "soap", DisplayOrder = 5, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Kitchen
                new() { Id = 15, Name = "Mini Fridge", Category = AmenityCategory.Kitchen, Icon = "kitchen", DisplayOrder = 1, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 16, Name = "Coffee Maker", Category = AmenityCategory.Kitchen, Icon = "coffee-maker", DisplayOrder = 2, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 17, Name = "Microwave", Category = AmenityCategory.Kitchen, Icon = "microwave", DisplayOrder = 3, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Safety
                new() { Id = 18, Name = "Safe", Category = AmenityCategory.Safety, Icon = "gpp-good", DisplayOrder = 1, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 19, Name = "Smoke Detector", Category = AmenityCategory.Safety, Icon = "smoke-free", DisplayOrder = 2, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new() { Id = 20, Name = "First Aid Kit", Category = AmenityCategory.Safety, Icon = "medical-services", DisplayOrder = 3, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            };

            modelBuilder.Entity<Amenity>().HasData(amenities);
        }
    }
}
