using BCrypt.Net;
using VisitaBookingApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add database and authentication services
builder.Services.AddAppDatabase(builder.Configuration);
builder.Services.AddAppAuthentication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

// Authentication must come before authorization
app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("VisitaP@ss2022"));
app.Run();

