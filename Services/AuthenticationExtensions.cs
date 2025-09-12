using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VisitaBookingApi.Data;
using VisitaBookingApi.Services.Interfaces;

namespace VisitaBookingApi.Services
{
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Configures JWT authentication and registers authentication services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Application configuration</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Register authentication service
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // Configure JWT Authentication
            var jwtSettings = configuration.GetSection("JWT");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is required"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, // Remove default 5 minute tolerance
                    NameClaimType = System.Security.Claims.ClaimTypes.Name,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };

                // Add event handlers for debugging and logging
                // options.Events = new JwtBearerEvents
                // {
                //     OnAuthenticationFailed = context =>
                //     {
                //         var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<AuthenticationService>>();
                //         logger.LogError("JWT Authentication failed: {Message}", context.Exception.Message);
                //         return Task.CompletedTask;
                //     },
                //     OnTokenValidated = context =>
                //     {
                //         var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<AuthenticationService>>();
                //         logger.LogDebug("JWT Token validated for user: {UserId}", 
                //             context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                //         return Task.CompletedTask;
                //     }
                // };
            });

            // Configure Authorization with role-based policies
            // services.AddAuthorization(options =>
            // {
            //     // Default policy requires authentication
            //     options.FallbackPolicy = options.DefaultPolicy;

            //     // Role-based policies
            //     options.AddPolicy("GuestPolicy", policy => 
            //         policy.RequireAuthenticatedUser().RequireRole("Guest"));
                
            //     options.AddPolicy("HotelPolicy", policy => 
            //         policy.RequireAuthenticatedUser().RequireRole("Hotel"));
                
            //     options.AddPolicy("AdminPolicy", policy => 
            //         policy.RequireAuthenticatedUser().RequireRole("Admin"));

            //     // Combined policies
            //     options.AddPolicy("HotelOrAdminPolicy", policy => 
            //         policy.RequireAuthenticatedUser().RequireRole("Hotel", "Admin"));
                
            //     options.AddPolicy("AllUsersPolicy", policy => 
            //         policy.RequireAuthenticatedUser().RequireRole("Guest", "Hotel", "Admin"));
            // });

            return services;
        }

        /// <summary>
        /// Configures Entity Framework with MySQL
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Application configuration</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Database connection string is required");

            Console.WriteLine(connectionString);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySQL(connectionString));

            return services;
        }
    }
}
