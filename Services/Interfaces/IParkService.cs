using Microsoft.AspNetCore.Http;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Services.Interfaces
{
    public interface IParkService
    {
        Task<List<ParkDto>> GetAllParksAsync();
        Task<List<ParkDto>> GetActiveParksAsync();
        Task<ParkDto?> GetParkByIdAsync(int id);
        Task<ParkDto> CreateParkAsync(CreateParkDto createDto, IFormFile? imageFile);
        Task<ParkDto> UpdateParkAsync(int id, UpdateParkDto updateDto, IFormFile? imageFile);
        Task<bool> DeleteParkAsync(int id);

        // Park Image management
        Task<ParkImageDto> AddParkImageAsync(int parkId, IFormFile imageFile, int displayOrder);
        Task<bool> DeleteParkImageAsync(int parkId, int imageId);
        Task<bool> UpdateParkImageOrderAsync(int parkId, int imageId, int newDisplayOrder);
    }
}
