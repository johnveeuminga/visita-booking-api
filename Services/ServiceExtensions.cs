using Amazon.S3;
using Amazon;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Services.Implementation;
using VisitaBookingApi.Services.Interfaces;

namespace visita_booking_api.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add AWS S3 client - uses AWS credential chain (AWS config files, IAM roles, environment vars)
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();
            
            // Register business services
            services.AddScoped<IRoomService, SimpleRoomService>();
            services.AddScoped<IRoomCalendarService, RoomCalendarService>();
            services.AddScoped<IRoomSearchService, RoomSearchService>();
            services.AddScoped<IAccommodationSearchService, AccommodationSearchService>();
            services.AddScoped<IRoomPriceCacheService, RoomPriceCacheService>();
            services.AddScoped<IAmenityService, SimpleAmenityService>();
            services.AddScoped<IS3FileService, SimpleS3FileService>();
            
            // Development data seeding service
            services.AddScoped<IDevelopmentDataSeedingService, DevelopmentDataSeedingService>();
            
            return services;
        }
    }
}