using Microsoft.EntityFrameworkCore;
using VisitaBookingApi.Data;
using VisitaBookingApi.Models;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Services.Implementation
{
    public class DevelopmentDataSeedingService : IDevelopmentDataSeedingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoomPriceCacheService _priceCacheService;
        private readonly ILogger<DevelopmentDataSeedingService> _logger;

        public DevelopmentDataSeedingService(
            ApplicationDbContext context,
            IRoomPriceCacheService priceCacheService,
            ILogger<DevelopmentDataSeedingService> logger)
        {
            _context = context;
            _priceCacheService = priceCacheService;
            _logger = logger;
        }

        public async Task SeedDevelopmentDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting development data seeding...");

                // Check if data already exists
                if (await _context.Hotels.AnyAsync())
                {
                    _logger.LogInformation("Development data already exists. Skipping seeding.");
                    return;
                }

                // Seed hotels
                var hotels = await SeedHotelsAsync(2);
                
                // Seed rooms for each hotel
                var rooms = new List<Room>();
                foreach (var hotel in hotels)
                {
                    var hotelRooms = await SeedRoomsForHotelAsync(hotel);
                    rooms.AddRange(hotelRooms);
                }

                // Add photos to rooms
                await SeedRoomPhotosAsync(rooms);

                // Add amenities to rooms
                await SeedRoomAmenitiesAsync(rooms);

                // Add some pricing rules
                await SeedPricingRulesAsync(rooms);


                // Pre-populate price caches
                await PrePopulatePriceCachesAsync(rooms);

                _logger.LogInformation("Development data seeding completed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding development data");
                throw;
            }
        }

        private async Task<User> SeedUsersAsync()
        {
            _logger.LogInformation("Seeding users...");

            // Check if any users already exist
            var existingUser = await _context.Users.FirstOrDefaultAsync();
            if (existingUser != null)
            {
                _logger.LogInformation("Users already exist. Using existing user ID: {UserId}", existingUser.Id);
                return existingUser;
            }

            var hotelOwner = new User
            {
                Email = "hotelowner@example.com",
                FirstName = "Hotel",
                LastName = "Owner",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("HotelOwner123!"),
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(hotelOwner);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded 1 hotel owner user with ID: {UserId}", hotelOwner.Id);
            return hotelOwner;
        }

        private async Task<List<Hotel>> SeedHotelsAsync(int ownerId)
        {
            _logger.LogInformation("Seeding hotels with owner ID: {OwnerId}", ownerId);

            var hotels = new List<Hotel>
            {
                new Hotel
                {
                    Id = 1,
                    Name = "Grand Marina Hotel",
                    Description = "A luxurious waterfront hotel with stunning bay views and world-class amenities. Perfect for business and leisure travelers.",
                    Address = "123 Marina Boulevard",
                    City = "Manila",
                    Country = "Philippines", 
                    PostalCode = "1000",
                    PhoneNumber = "+63-2-555-0101",
                    Email = "info@grandmarina.ph",
                    Website = "https://www.grandmarinahotel.ph",
                    Rating = 4.5m,
                    ReviewCount = 1247,
                    HasParking = true,
                    HasWifi = true,
                    HasPool = true,
                    HasGym = true,
                    HasSpa = true,
                    HasRestaurant = true,
                    CheckInTime = new TimeSpan(15, 0, 0), // 3:00 PM
                    CheckOutTime = new TimeSpan(12, 0, 0), // 12:00 PM
                    IsActive = true,
                    IsVerified = true,
                    OwnerId = null, // No owner for now - will fix this later
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Hotel
                {
                    Id = 2,
                    Name = "Cozy Mountain Retreat",
                    Description = "A charming boutique hotel nestled in the mountains, offering peaceful surroundings and personalized service.",
                    Address = "456 Mountain View Road",
                    City = "Baguio",
                    Country = "Philippines",
                    PostalCode = "2600",
                    PhoneNumber = "+63-74-555-0202",
                    Email = "reservations@cozymountain.ph",
                    Website = "https://www.cozymountainretreat.ph",
                    Rating = 4.2m,
                    ReviewCount = 892,
                    HasParking = true,
                    HasWifi = true,
                    HasPool = false,
                    HasGym = false,
                    HasSpa = true,
                    HasRestaurant = true,
                    CheckInTime = new TimeSpan(14, 0, 0), // 2:00 PM
                    CheckOutTime = new TimeSpan(11, 0, 0), // 11:00 AM
                    IsActive = true,
                    IsVerified = true,
                    OwnerId = ownerId, // Use the actual owner ID
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _context.Hotels.AddRange(hotels);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} hotels", hotels.Count);
            return hotels;
        }

        private async Task<List<Room>> SeedRoomsForHotelAsync(Hotel hotel)
        {
            _logger.LogInformation("Seeding rooms for hotel: {HotelName}", hotel.Name);

            var rooms = new List<Room>();
            var roomTypes = hotel.Id == 1 ? 
                // Grand Marina Hotel - luxury rooms
                new[] 
                {
                    ("Deluxe Ocean View", "Spacious room with panoramic ocean views and modern amenities", 3500m, 2),
                    ("Executive Suite", "Premium suite with separate living area and executive privileges", 5200m, 4),
                    ("Presidential Suite", "The ultimate luxury experience with private balcony and butler service", 8900m, 4),
                    ("Standard City View", "Comfortable accommodation with city skyline views", 2800m, 2),
                    ("Family Room", "Perfect for families with connecting rooms and extra space", 4100m, 6)
                } :
                // Cozy Mountain Retreat - boutique rooms  
                new[]
                {
                    ("Mountain View Single", "Cozy room with stunning mountain vistas", 1800m, 1),
                    ("Garden View Double", "Peaceful room overlooking the hotel gardens", 2400m, 2), 
                    ("Deluxe Mountain Suite", "Spacious suite with fireplace and mountain panorama", 3600m, 3),
                    ("Family Cabin", "Rustic family accommodation with kitchenette", 3200m, 5),
                    ("Romantic Hideaway", "Intimate room perfect for couples with private terrace", 2900m, 2)
                };

            for (int i = 0; i < roomTypes.Length; i++)
            {
                var (name, description, price, guests) = roomTypes[i];
                var room = new Room
                {
                    Name = name,
                    Description = description,
                    DefaultPrice = price,
                    MaxGuests = guests,
                    HotelId = hotel.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CacheVersion = 1
                };

                rooms.Add(room);
            }

            _context.Rooms.AddRange(rooms);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} rooms for {HotelName}", rooms.Count, hotel.Name);
            return rooms;
        }

        private async Task SeedRoomPhotosAsync(List<Room> rooms)
        {
            _logger.LogInformation("Seeding room photos...");

            var photos = new List<RoomPhoto>();
            var photoBaseUrls = new[]
            {
                "https://images.unsplash.com/photo-1611892440504-42a792e24d32", // Hotel room 1
                "https://images.unsplash.com/photo-1618773928121-c32242e63f39", // Hotel room 2  
                "https://images.unsplash.com/photo-1590490360182-c33d57733427", // Hotel room 3
                "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b", // Hotel room 4
                "https://images.unsplash.com/photo-1566665797739-1674de7a421a", // Hotel room 5
                "https://images.unsplash.com/photo-1631049307264-da0ec9d70304", // Hotel room 6
                "https://images.unsplash.com/photo-1564013799919-ab600027ffc6", // Hotel room 7
                "https://images.unsplash.com/photo-1595576508898-0ad5c879a061", // Hotel room 8
            };

            foreach (var room in rooms)
            {
                // Add 2-3 photos per room
                var photoCount = Random.Shared.Next(2, 4);
                for (int i = 0; i < photoCount; i++)
                {
                    var photoUrl = photoBaseUrls[Random.Shared.Next(photoBaseUrls.Length)];
                    var photo = new RoomPhoto
                    {
                        RoomId = room.Id,
                        S3Key = $"rooms/{room.Id}/photo-{i + 1}.jpg",
                        S3Url = $"{photoUrl}?w=1200&h=800&fit=crop",
                        CdnUrl = $"{photoUrl}?w=1200&h=800&fit=crop&auto=format",
                        FileName = $"room-{room.Id}-photo-{i + 1}.jpg",
                        FileSize = Random.Shared.Next(100000, 500000), // Random file size between 100KB-500KB
                        ContentType = "image/jpeg",
                        DisplayOrder = i + 1,
                        AltText = $"{room.Name} - Photo {i + 1}",
                        IsActive = true,
                        UploadedAt = DateTime.UtcNow,
                        LastModified = DateTime.UtcNow
                    };
                    photos.Add(photo);
                }
            }

            _context.RoomPhotos.AddRange(photos);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} room photos", photos.Count);
        }

        private async Task SeedRoomAmenitiesAsync(List<Room> rooms)
        {
            _logger.LogInformation("Seeding room amenities...");

            var amenities = await _context.Amenities.ToListAsync();
            var roomAmenities = new List<RoomAmenity>();

            foreach (var room in rooms)
            {
                // Assign random amenities based on room type
                var amenityCount = room.DefaultPrice switch
                {
                    < 2000m => Random.Shared.Next(3, 6),   // Basic rooms: 3-5 amenities
                    < 4000m => Random.Shared.Next(6, 10),  // Mid-range: 6-9 amenities  
                    _ => Random.Shared.Next(10, 15)         // Luxury: 10-14 amenities
                };

                var selectedAmenities = amenities
                    .OrderBy(x => Random.Shared.Next())
                    .Take(amenityCount)
                    .ToList();

                foreach (var amenity in selectedAmenities)
                {
                    var roomAmenity = new RoomAmenity
                    {
                        RoomId = room.Id,
                        AmenityId = amenity.Id,
                        AssignedAt = DateTime.UtcNow,
                        Notes = null
                    };
                    roomAmenities.Add(roomAmenity);
                }
            }

            _context.RoomAmenities.AddRange(roomAmenities);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} room-amenity relationships", roomAmenities.Count);
        }

        private async Task SeedPricingRulesAsync(List<Room> rooms)
        {
            _logger.LogInformation("Seeding pricing rules...");

            var pricingRules = new List<RoomPricingRule>();
            var now = DateTime.UtcNow;

            foreach (var room in rooms)
            {
                // Weekend pricing (Friday-Sunday)
                var weekendRule = new RoomPricingRule
                {
                    RoomId = room.Id,
                    RuleType = PricingRuleType.Weekend,
                    Name = "Weekend Premium",
                    Description = "Weekend pricing with 20% increase",
                    DayOfWeek = 5, // Friday
                    FixedPrice = room.DefaultPrice * 1.2m,
                    IsActive = true,
                    Priority = 10,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                pricingRules.Add(weekendRule);

                // Holiday season pricing (December)
                var holidayRule = new RoomPricingRule
                {
                    RoomId = room.Id,
                    RuleType = PricingRuleType.Holiday,
                    Name = "Holiday Season",
                    Description = "Christmas and New Year premium pricing",
                    StartDate = new DateTime(now.Year, 12, 20),
                    EndDate = new DateTime(now.Year + 1, 1, 5),
                    FixedPrice = room.DefaultPrice * 1.5m,
                    IsActive = true,
                    Priority = 20,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                pricingRules.Add(holidayRule);

                // Summer season (March-May)
                var summerRule = new RoomPricingRule
                {
                    RoomId = room.Id,
                    RuleType = PricingRuleType.Seasonal,
                    Name = "Summer Season",
                    Description = "Peak summer season pricing",
                    StartDate = new DateTime(now.Year, 3, 1),
                    EndDate = new DateTime(now.Year, 5, 31),
                    FixedPrice = room.DefaultPrice * 1.15m,
                    IsActive = true,
                    Priority = 5,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                pricingRules.Add(summerRule);
            }

            _context.RoomPricingRules.AddRange(pricingRules);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} pricing rules", pricingRules.Count);
        }

        private async Task PrePopulatePriceCachesAsync(List<Room> rooms)
        {
            _logger.LogInformation("Pre-populating price caches...");

            var roomIds = rooms.Select(r => r.Id).ToList();
            await _priceCacheService.UpdateRoomsPriceCacheAsync(roomIds);

            _logger.LogInformation("Pre-populated price caches for {Count} rooms", roomIds.Count);
        }
    }
}