using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VisitaBookingAPI.DTOs;

namespace VisitaBookingAPI.Services.Interfaces
{
    public interface IEstablishmentService
    {
        Task<EstablishmentSearchResult> SearchEstablishmentsAsync(
            EstablishmentSearchParams searchParams
        );
        Task<EstablishmentDetailDto?> GetEstablishmentByIdAsync(int id);
        Task<List<EstablishmentCategoryDto>> GetCategoriesWithSubcategoriesAsync();

        Task<List<EstablishmentListDto>> GetMyEstablishmentsAsync(int ownerId);
        Task<EstablishmentDetailDto> CreateEstablishmentAsync(
            int ownerId,
            CreateEstablishmentDto dto
        );
        Task<EstablishmentDetailDto> UpdateEstablishmentAsync(
            int establishmentId,
            int ownerId,
            UpdateEstablishmentDto dto
        );
        Task<bool> DeleteEstablishmentAsync(int establishmentId, int ownerId);
        Task<string> UploadLogoAsync(int establishmentId, int ownerId, IFormFile file);
        Task<string> UploadCoverImageAsync(int establishmentId, int ownerId, IFormFile file);
        Task<EstablishmentImageDto> AddEstablishmentImageAsync(
            int establishmentId,
            int ownerId,
            IFormFile file,
            string? caption,
            int displayOrder
        );
        Task<bool> DeleteEstablishmentImageAsync(int establishmentId, int imageId, int ownerId);
        Task<string> UploadBusinessPermitAsync(int establishmentId, int ownerId, IFormFile file);

        Task<EstablishmentMenuItemDto> AddMenuItemAsync(
            int establishmentId,
            int ownerId,
            CreateMenuItemDto dto
        );
        Task<EstablishmentMenuItemDto> UpdateMenuItemAsync(
            int establishmentId,
            int menuItemId,
            int ownerId,
            UpdateMenuItemDto dto
        );
        Task<bool> DeleteMenuItemAsync(int establishmentId, int menuItemId, int ownerId);
        Task<string> UploadMenuItemImageAsync(
            int establishmentId,
            int menuItemId,
            int ownerId,
            IFormFile file
        );

        Task<List<AdminEstablishmentListDto>> GetPendingEstablishmentsAsync();
        Task<List<AdminEstablishmentListDto>> GetAllEstablishmentsAsync(
            string? status,
            string? category
        );
        Task<bool> ApproveEstablishmentAsync(int establishmentId, int adminId);
        Task<bool> RejectEstablishmentAsync(
            int establishmentId,
            int adminId,
            string rejectionReason
        );
        Task<string?> GetBusinessPermitDownloadUrlAsync(int establishmentId);
    }
}
