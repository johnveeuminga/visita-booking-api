using StackExchange.Redis;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Services.Implementation;

namespace visita_booking_api.Services
{
    public static class CachingExtensions
    {
        public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Redis Cache for Upstash (TCP connection)
            var redisConnectionString = configuration.GetConnectionString("Redis") 
                ?? throw new ArgumentNullException("Redis connection string is required");

            Console.WriteLine($"Using Redis Cache at: {redisConnectionString}");
            
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "visita-booking-api";
            });

            // Register Redis ConnectionMultiplexer for direct Redis operations (TCP)
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var connectionString = configuration.GetConnectionString("Redis")!;
                return ConnectionMultiplexer.Connect(connectionString);
            });

            // Register the Redis-only cache service
            services.AddScoped<ICacheService, RedisCacheService>();
            
            // Register cache invalidation service
            services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
            
            // Register cache warmup service
            services.AddScoped<ICacheWarmupService, CacheWarmupService>();

            // Add response caching for HTTP responses
            services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 1024 * 1024; // 1MB
                options.UseCaseSensitivePaths = true;
            });

            return services;
        }
    }
}