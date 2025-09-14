using BCrypt.Net;
using VisitaBookingApi.Services;
using visita_booking_api.Services;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS policy
builder.Services.AddCors(options =>
{
    var corsConfig = builder.Configuration.GetSection("CORS");
    var allowedOrigins = corsConfig.GetSection("AllowedOrigins").Get<string[]>() ?? new string[]
    {
        "http://localhost:3000",
        "http://localhost:3001",
        "http://localhost:4200",
        "http://localhost:8080",
        "http://localhost:8000",
        "https://localhost:3000",
        "https://localhost:3001",
        "https://localhost:4200"
    };
    
    var productionOrigins = corsConfig.GetSection("ProductionOrigins").Get<string[]>() ?? new string[]
    {
        "https://yourdomain.com",
        "https://www.yourdomain.com"
    };

    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Allow cookies and authorization headers
    });

    // More restrictive policy for production
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(productionOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add database and authentication services
builder.Services.AddAppDatabase(builder.Configuration);
builder.Services.AddAppAuthentication(builder.Configuration);

// Add caching services
builder.Services.AddAppCaching(builder.Configuration);

// Add AutoMapper
builder.Services.AddAutoMapper(config => {
    config.AddMaps(typeof(Program).Assembly);
});

// Add business services
builder.Services.AddAppServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable CORS
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowFrontend");
}
else
{
    app.UseCors("Production");
}

// app.UseHttpsRedirection();

// Use caching middleware
app.UseAppCaching(builder.Configuration);

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Data seeding - Only when SEED_DATA environment variable is set to true
var seedData = Environment.GetEnvironmentVariable("SEED_DATA");
if (!string.IsNullOrEmpty(seedData) && seedData.Equals("true", StringComparison.OrdinalIgnoreCase))
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            // Ensure database exists and is created
            logger.LogInformation("Ensuring database is created...");
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // This will create the database if it doesn't exist
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database creation completed successfully");
            
            // Now seed development data
            logger.LogInformation("Starting development data seeding...");
            var seedingService = scope.ServiceProvider.GetRequiredService<IDevelopmentDataSeedingService>();
            await seedingService.SeedDevelopmentDataAsync();
            logger.LogInformation("Development data seeding completed successfully");
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while creating/migrating the database or seeding development data");
            throw;
        }
    }
}

Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("VisitaP@ss2022"));
app.Run();
