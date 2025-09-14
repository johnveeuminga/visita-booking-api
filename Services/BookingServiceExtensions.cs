using Microsoft.Extensions.DependencyInjection;
using visita_booking_api.Services.Implementation;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Services.Interfaces;
using VisitaBookingApi.Services.Implementation;
using StackExchange.Redis;

namespace visita_booking_api.Services
{
    public static class BookingServiceExtensions
    {
        /// <summary>
        /// Registers all booking-related services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddBookingServices(this IServiceCollection services)
        {
            // Register Redis connection for distributed locking
            services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
                return ConnectionMultiplexer.Connect(connectionString);
            });

            // Register distributed lock service
            services.AddScoped<IDistributedLockService, RedisDistributedLockService>();

            // Register Xendit service with HttpClient
            services.AddHttpClient<IXenditService, XenditService>()
                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    // HttpClient configuration is handled in XenditService constructor
                });

            // Register booking service
            services.AddScoped<IBookingService, BookingService>();

            // Register background service for maintenance tasks
            services.AddHostedService<BookingMaintenanceService>();

            return services;
        }

        /// <summary>
        /// Configures booking-related options from configuration
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection ConfigureBookingOptions(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Xendit options
            services.Configure<XenditConfiguration>(
                configuration.GetSection("Xendit"));

            // Configure Booking options  
            services.Configure<BookingConfiguration>(
                configuration.GetSection("Booking"));

            return services;
        }
    }
}