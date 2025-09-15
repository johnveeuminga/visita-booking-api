using visita_booking_api.Services.Interfaces;
using visita_booking_api.Services.Implementation;

namespace visita_booking_api.Services
{
    public static class CachingExtensions
    {
        public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Redis Cache for Upstash (HTTP REST)
            var redisConnectionString = configuration.GetConnectionString("RedisConnection") 
                ?? throw new ArgumentNullException("Redis connection string is required");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "visita-booking-api";
            });

            // Register the Redis-only cache service (no ConnectionMultiplexer needed for Upstash)
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