using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VisitaBookingApi.Data;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;
using visita_booking_api.Models.Cache;

namespace visita_booking_api.Services.Implementation
{
    public class SimpleAmenityService : IAmenityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SimpleAmenityService> _logger;

        public SimpleAmenityService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<SimpleAmenityService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AmenityDTO?> GetByIdAsync(int amenityId)
        {
            var amenity = await _context.Amenities.FindAsync(amenityId);
            return amenity == null ? null : _mapper.Map<AmenityDTO>(amenity);
        }

        public async Task<List<AmenityDTO>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Amenities.AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }

            var amenities = await query
                .OrderBy(a => a.Category)
                .ThenBy(a => a.DisplayOrder)
                .ThenBy(a => a.Name)
                .ToListAsync();

            return _mapper.Map<List<AmenityDTO>>(amenities);
        }

        public async Task<List<AmenityDTO>> GetByCategoryAsync(AmenityCategory category)
        {
            var amenities = await _context.Amenities
                .Where(a => a.Category == category && a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.Name)
                .ToListAsync();

            return _mapper.Map<List<AmenityDTO>>(amenities);
        }

        public async Task<Dictionary<string, List<AmenityDTO>>> GetGroupedByCategoryAsync(bool includeInactive = false)
        {
            var amenities = await GetAllAsync(includeInactive);
            
            return amenities
                .GroupBy(a => a.Category.ToString())
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Name).ToList());
        }

        public async Task<AmenityDTO> CreateAsync(AmenityCreateDTO createDto)
        {
            var amenity = _mapper.Map<Amenity>(createDto);
            amenity.CreatedAt = DateTime.UtcNow;
            amenity.LastModified = DateTime.UtcNow;

            if (amenity.DisplayOrder == 0)
            {
                var maxOrder = await _context.Amenities
                    .Where(a => a.Category == amenity.Category)
                    .MaxAsync(a => (int?)a.DisplayOrder) ?? 0;
                amenity.DisplayOrder = maxOrder + 1;
            }

            _context.Amenities.Add(amenity);
            await _context.SaveChangesAsync();

            return _mapper.Map<AmenityDTO>(amenity);
        }

        public async Task<AmenityDTO?> UpdateAsync(int amenityId, AmenityCreateDTO updateDto)
        {
            var amenity = await _context.Amenities.FindAsync(amenityId);
            if (amenity == null) return null;

            _mapper.Map(updateDto, amenity);
            amenity.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<AmenityDTO>(amenity);
        }

        public async Task<bool> DeleteAsync(int amenityId)
        {
            var amenity = await _context.Amenities
                .Include(a => a.RoomAmenities)
                .FirstOrDefaultAsync(a => a.Id == amenityId);
            
            if (amenity == null) return false;

            if (amenity.RoomAmenities.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete amenity '{amenity.Name}' as it is assigned to {amenity.RoomAmenities.Count} room(s).");
            }

            _context.Amenities.Remove(amenity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveStatusAsync(int amenityId)
        {
            var amenity = await _context.Amenities.FindAsync(amenityId);
            if (amenity == null) return false;

            amenity.IsActive = !amenity.IsActive;
            amenity.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderAmenitiesAsync(string category, List<int> amenityIds)
        {
            if (!Enum.TryParse<AmenityCategory>(category, out var categoryEnum))
                return false;

            var amenities = await _context.Amenities
                .Where(a => a.Category == categoryEnum && amenityIds.Contains(a.Id))
                .ToListAsync();

            if (amenities.Count != amenityIds.Count)
                return false;

            for (int i = 0; i < amenityIds.Count; i++)
            {
                var amenity = amenities.First(a => a.Id == amenityIds[i]);
                amenity.DisplayOrder = i;
                amenity.LastModified = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            var categories = await _context.Amenities
                .Where(a => a.IsActive)
                .Select(a => a.Category)
                .Distinct()
                .ToListAsync();

            return categories.Select(c => c.ToString()).OrderBy(c => c).ToList();
        }

        public async Task<List<AmenityDTO>> SearchAsync(string searchTerm, string? category = null, bool includeInactive = false)
        {
            var query = _context.Amenities.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<AmenityCategory>(category, out var categoryEnum))
            {
                query = query.Where(a => a.Category == categoryEnum);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(a => 
                    a.Name.ToLower().Contains(lowerSearchTerm) ||
                    a.Description.ToLower().Contains(lowerSearchTerm));
            }

            var amenities = await query
                .OrderBy(a => a.Category)
                .ThenBy(a => a.DisplayOrder)
                .ThenBy(a => a.Name)
                .ToListAsync();

            return _mapper.Map<List<AmenityDTO>>(amenities);
        }

        public async Task<int> GetUsageCountAsync(int amenityId)
        {
            return await _context.RoomAmenities
                .Where(ra => ra.AmenityId == amenityId)
                .CountAsync();
        }

        public async Task<List<AmenityUsageDTO>> GetUsageStatsAsync()
        {
            var stats = await _context.Amenities
                .Select(a => new AmenityUsageDTO
                {
                    AmenityId = a.Id,
                    Name = a.Name,
                    Category = a.Category.ToString(),
                    UsageCount = a.RoomAmenities.Count,
                    IsActive = a.IsActive
                })
                .OrderByDescending(u => u.UsageCount)
                .ThenBy(u => u.Category)
                .ThenBy(u => u.Name)
                .ToListAsync();

            return stats;
        }

        public async Task<List<AmenityDTO>> GetParentAmenitiesAsync()
        {
            var amenities = await _context.Amenities
                .Where(a => a.ParentAmenityId == null && a.IsActive)
                .OrderBy(a => a.Category)
                .ThenBy(a => a.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<AmenityDTO>>(amenities);
        }

        public async Task<List<AmenityDTO>> GetChildAmenitiesAsync(int parentAmenityId)
        {
            var amenities = await _context.Amenities
                .Where(a => a.ParentAmenityId == parentAmenityId && a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.Name)
                .ToListAsync();

            return _mapper.Map<List<AmenityDTO>>(amenities);
        }

        public async Task<bool> ExistsAsync(int amenityId)
        {
            return await _context.Amenities.AnyAsync(a => a.Id == amenityId);
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeAmenityId = null)
        {
            var query = _context.Amenities.Where(a => a.Name == name);
            
            if (excludeAmenityId.HasValue)
            {
                query = query.Where(a => a.Id != excludeAmenityId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}