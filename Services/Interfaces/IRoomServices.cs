using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Services.Interfaces
{
    public interface IRoomService
    {
        // CRUD Operations
        Task<RoomDetailsDTO?> GetByIdAsync(int roomId);
        Task<PaginatedResponse<RoomListItemDTO>> GetAllAsync(int page = 1, int pageSize = 10, bool includeInactive = false);
        Task<RoomDetailsDTO> CreateAsync(RoomCreateDTO createDto);
        Task<RoomDetailsDTO?> UpdateAsync(int roomId, RoomUpdateDTO updateDto);
        Task<bool> DeleteAsync(int roomId);
        Task<bool> ToggleActiveStatusAsync(int roomId);

        // Photo Management
        Task<List<RoomPhotoDTO>> GetPhotosAsync(int roomId);
        
        [Obsolete("Use CreateAsync or UpdateAsync with PhotoFiles property for integrated photo upload")]
        Task<RoomPhotoDTO> UploadPhotoAsync(int roomId, RoomPhotoUploadDTO uploadDto);
        
        Task<bool> DeletePhotoAsync(int roomId, int photoId);
        Task<bool> ReorderPhotosAsync(int roomId, List<int> photoIds);

        // Amenity Management
        Task<List<AmenityDTO>> GetRoomAmenitiesAsync(int roomId);
        Task<bool> UpdateRoomAmenitiesAsync(int roomId, List<int> amenityIds);

        // Validation
        Task<bool> ExistsAsync(int roomId);
        Task<bool> IsNameUniqueAsync(string name, int? excludeRoomId = null);
    }

    public interface IAmenityService
    {
        // CRUD Operations
        Task<List<AmenityDTO>> GetAllAsync(bool includeInactive = false);
        Task<List<AmenityDTO>> GetByCategoryAsync(AmenityCategory category);
        Task<AmenityDTO?> GetByIdAsync(int amenityId);
        Task<AmenityDTO> CreateAsync(AmenityCreateDTO createDto);
        Task<AmenityDTO?> UpdateAsync(int amenityId, AmenityCreateDTO updateDto);
        Task<bool> DeleteAsync(int amenityId);

        // Hierarchy Management
        Task<List<AmenityDTO>> GetParentAmenitiesAsync();
        Task<List<AmenityDTO>> GetChildAmenitiesAsync(int parentAmenityId);

        // Validation
        Task<bool> ExistsAsync(int amenityId);
        Task<bool> IsNameUniqueAsync(string name, int? excludeAmenityId = null);
    }

    public interface IS3FileService
    {
        Task<FileUploadResponse> UploadFileAsync(IFormFile file, string folder);
        Task<FileUploadResponse> UploadPrivateFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string s3Key);
        Task<string> GetPresignedUrlAsync(string s3Key, TimeSpan expiration);
        Task<List<FileUploadResponse>> UploadMultipleFilesAsync(List<IFormFile> files, string folder);
        string ExtractS3KeyFromUrl(string fileUrl);
    }
}