using Amazon;
using Amazon.S3;
using visita_booking_api.Services.Implementation;
using visita_booking_api.Services.Interfaces;
using VisitaBookingAPI.Services;
using VisitaBookingApi.Services.Interfaces;
using VisitaBookingAPI.Services.Interfaces;

namespace visita_booking_api.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddAppServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
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
            services.AddScoped<IAvailabilityLedgerService, AvailabilityLedgerService>();
            services.AddScoped<IEmailService, SendGridEmailService>();
            services.AddScoped<IRefundService, RefundService>();
            services.AddScoped<IBulletinEventService, BulletinEventService>();
            services.AddScoped<IParkService, ParkService>();
            services.AddScoped<IEstablishmentService, EstablishmentService>();

            // Development data seeding service
            services.AddScoped<IDevelopmentDataSeedingService, DevelopmentDataSeedingService>();

            return services;
        }
    }
}
