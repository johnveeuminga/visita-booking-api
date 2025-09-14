using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using visita_booking_api.Services.Implementation;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Services
{
    public static class CachingExtensions
    {
        public static IServiceCollection AddAppCaching(this IServiceCollection services, IConfiguration configuration)
        {
            // Memory Cache
            services.AddMemoryCache(options =>
            {
                var maxSizeMB = configuration.GetValue<int>("Caching:MaxMemoryCacheSizeMB");
                if (maxSizeMB > 0)
                {
                    options.SizeLimit = maxSizeMB * 1024 * 1024; // Convert MB to bytes
                }
                options.TrackStatistics = true;
            });

            // Response Caching
            var useResponseCaching = configuration.GetValue<bool>("Caching:UseResponseCaching");
            if (useResponseCaching)
            {
                services.AddResponseCaching();
            }

            // Redis Configuration
            var useRedis = configuration.GetValue<bool>("Caching:UseRedis");
            if (useRedis)
            {
                var redisConnection = configuration.GetConnectionString("RedisConnection");
                if (!string.IsNullOrEmpty(redisConnection))
                {
                    // Configure Redis connection
                    services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                        try
                        {
                            var configuration = ConfigurationOptions.Parse(redisConnection);
                            configuration.AbortOnConnectFail = false;
                            configuration.ConnectRetry = 3;
                            configuration.ConnectTimeout = 5000;
                            configuration.SyncTimeout = 5000;
                            
                            return ConnectionMultiplexer.Connect(configuration);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to connect to Redis. Continuing without Redis caching.");
                            return null!;
                        }
                    });

                    // Add Redis distributed cache
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = redisConnection;
                        options.InstanceName = "VisitaBookingApi";
                    });
                }
                else
                {
                    services.AddSingleton<IConnectionMultiplexer>(_ => null!);
                    services.AddDistributedMemoryCache(); // Fallback to in-memory distributed cache
                }
            }
            else
            {
                services.AddSingleton<IConnectionMultiplexer>(_ => null!);
                services.AddDistributedMemoryCache(); // Use in-memory distributed cache
            }

            // Register cache services
            services.AddScoped<ICacheService, HybridCacheService>();
            services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
            services.AddScoped<ICacheWarmupService, CacheWarmupService>();

            // Add background service for cache warming
            services.AddHostedService<CacheWarmupBackgroundService>();

            return services;
        }

        public static IApplicationBuilder UseAppCaching(this IApplicationBuilder app, IConfiguration configuration)
        {
            var useResponseCaching = configuration.GetValue<bool>("Caching:UseResponseCaching");
            if (useResponseCaching)
            {
                app.UseResponseCaching();
            }

            return app;
        }
    }
}