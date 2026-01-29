using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using visita_booking_api.Models.Entities;
using visita_booking_api.Models.Enums;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;
using VisitaBookingAPI.DTOs;
using VisitaBookingAPI.Services.Interfaces;

namespace VisitaBookingAPI.Services
{
    public class EstablishmentService : IEstablishmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IS3FileService _s3Service;

        public EstablishmentService(ApplicationDbContext context, IS3FileService s3Service)
        {
            _context = context;
            _s3Service = s3Service;
        }

        // ==================== PUBLIC ENDPOINTS ====================

        public async Task<EstablishmentSearchResult> SearchEstablishmentsAsync(
            EstablishmentSearchParams searchParams
        )
        {
            var query = _context
                .Establishments.Include(e => e.Owner)
                .Include(e => e.Images)
                .Include(e => e.Subcategories)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(searchParams.Status))
            {
                if (Enum.TryParse<EstablishmentStatus>(searchParams.Status, out var status))
                {
                    query = query.Where(e => e.Status == status);
                }
            }

            // Filter by active status
            if (searchParams.IsActive.HasValue)
            {
                query = query.Where(e => e.IsActive == searchParams.IsActive.Value);
            }

            // Filter by category
            if (!string.IsNullOrEmpty(searchParams.Category))
            {
                if (Enum.TryParse<EstablishmentCategory>(searchParams.Category, out var category))
                {
                    query = query.Where(e => e.Category == category);
                }
            }

            // Filter by subcategories
            if (searchParams.SubcategoryIds != null && searchParams.SubcategoryIds.Any())
            {
                query = query.Where(e =>
                    e.Subcategories.Any(sc => searchParams.SubcategoryIds.Contains(sc.Id))
                );
            }

            // Filter by city
            if (!string.IsNullOrEmpty(searchParams.City))
            {
                query = query.Where(e => e.City == searchParams.City);
            }

            // Search term
            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                var searchLower = searchParams.SearchTerm.ToLower();
                query = query.Where(e =>
                    e.Name.ToLower().Contains(searchLower)
                    || (e.Description != null && e.Description.ToLower().Contains(searchLower))
                    || e.Address.ToLower().Contains(searchLower)
                );
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Sorting
            query = searchParams.SortBy?.ToLower() switch
            {
                "created_at" => searchParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.CreatedAt)
                    : query.OrderBy(e => e.CreatedAt),
                _ => searchParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Name)
                    : query.OrderBy(e => e.Name),
            };

            // Pagination
            var establishments = await query
                .Skip((searchParams.Page - 1) * searchParams.PageSize)
                .Take(searchParams.PageSize)
                .Select(e => new EstablishmentListDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Category = e.Category.ToString(),
                    Subcategories = e.Subcategories.Select(sc => sc.Name).ToList(),
                    Logo = e.Logo,
                    CoverImage =
                        e.CoverImage
                        ?? e.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()!.ImageUrl,
                    Address = e.Address,
                    City = e.City,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    ContactNumber = e.ContactNumber,
                    Status = e.Status.ToString(),
                    IsActive = e.IsActive,
                    CreatedAt = e.CreatedAt,
                })
                .ToListAsync();

            return new EstablishmentSearchResult
            {
                Establishments = establishments,
                TotalCount = totalCount,
                Page = searchParams.Page,
                PageSize = searchParams.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)searchParams.PageSize),
            };
        }

        public async Task<EstablishmentDetailDto?> GetEstablishmentByIdAsync(int id)
        {
            var establishment = await _context
                .Establishments.Include(e => e.Owner)
                .Include(e => e.Images)
                .Include(e => e.Hours)
                .Include(e => e.Subcategories)
                .Include(e => e.MenuItems)
                .Include(e => e.Comments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (establishment == null)
            {
                return null;
            }

            return new EstablishmentDetailDto
            {
                Id = establishment.Id,
                Name = establishment.Name,
                Description = establishment.Description,
                Category = establishment.Category.ToString(),
                Subcategories = establishment
                    .Subcategories.Select(sc => new EstablishmentSubcategoryDto
                    {
                        Id = sc.Id,
                        CategoryId = sc.CategoryId,
                        Name = sc.Name,
                        Description = sc.Description,
                        DisplayOrder = sc.DisplayOrder,
                    })
                    .ToList(),
                Logo = establishment.Logo,
                CoverImage = establishment.CoverImage,
                Images = establishment
                    .Images.OrderBy(i => i.DisplayOrder)
                    .Select(i => new EstablishmentImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        DisplayOrder = i.DisplayOrder,
                        Caption = i.Caption,
                    })
                    .ToList(),
                Address = establishment.Address,
                City = establishment.City,
                Latitude = establishment.Latitude,
                Longitude = establishment.Longitude,
                ContactNumber = establishment.ContactNumber,
                Email = establishment.Email,
                Website = establishment.Website,
                FacebookPage = establishment.FacebookPage,
                OperatingHours =
                    establishment
                        .Hours?.OrderBy(h => GetDayOrder(h.DayOfWeek))
                        .Select(h => new EstablishmentHoursDto
                        {
                            Id = h.Id,
                            DayOfWeek = h.DayOfWeek.ToString(),
                            OpenTime = h.OpenTime,
                            CloseTime = h.CloseTime,
                            IsClosed = h.IsClosed,
                        })
                        .ToList() ?? new List<EstablishmentHoursDto>(),
                MenuItems = establishment
                    .MenuItems.Where(m => m.IsAvailable)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => new EstablishmentMenuItemDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        Price = m.Price,
                        Category = m.Category,
                        ImageUrl = m.ImageUrl,
                        IsAvailable = m.IsAvailable,
                        DisplayOrder = m.DisplayOrder,
                    })
                    .ToList(),
                Owner = new EstablishmentOwnerDto
                {
                    Id = establishment.Owner.Id,
                    FirstName = establishment.Owner.FirstName,
                    LastName = establishment.Owner.LastName,
                    Email = establishment.Owner.Email,
                },
                Status = establishment.Status.ToString(),
                IsActive = establishment.IsActive,
                CreatedAt = establishment.CreatedAt,
                UpdatedAt = establishment.UpdatedAt,
                CommentCount = establishment.Comments.Count,
                AverageRating = null,
            };
        }

        public async Task<List<EstablishmentCategoryDto>> GetCategoriesWithSubcategoriesAsync()
        {
            var categories = await _context
                .EstablishmentCategories.Include(c => c.Subcategories)
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return categories
                .Select(c => new EstablishmentCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Icon = c.Icon,
                    DisplayOrder = c.DisplayOrder,
                    Subcategories = c
                        .Subcategories.Where(sc => sc.IsActive)
                        .OrderBy(sc => sc.DisplayOrder)
                        .Select(sc => new EstablishmentSubcategoryDto
                        {
                            Id = sc.Id,
                            CategoryId = sc.CategoryId,
                            Name = sc.Name,
                            Description = sc.Description,
                            DisplayOrder = sc.DisplayOrder,
                        })
                        .ToList(),
                })
                .ToList();
        }

        // ==================== OWNER ENDPOINTS ====================

        public async Task<List<EstablishmentListDto>> GetMyEstablishmentsAsync(int ownerId)
        {
            var establishments = await _context
                .Establishments.Include(e => e.Subcategories)
                .Include(e => e.Images)
                .Where(e => e.OwnerId == ownerId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return establishments
                .Select(e => new EstablishmentListDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Category = e.Category.ToString(),
                    Subcategories = e.Subcategories.Select(sc => sc.Name).ToList(),
                    Logo = e.Logo,
                    CoverImage = e.CoverImage,
                    Address = e.Address,
                    City = e.City,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    ContactNumber = e.ContactNumber,
                    Status = e.Status.ToString(),
                    IsActive = e.IsActive,
                    CreatedAt = e.CreatedAt,
                })
                .ToList();
        }

        public async Task<EstablishmentDetailDto> CreateEstablishmentAsync(
            int ownerId,
            CreateEstablishmentDto dto
        )
        {
            // Verify owner has Establishment role
            var user = await _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == ownerId);

            if (user == null || !user.UserRoles.Any(ur => ur.Role.Name == "Establishment"))
            {
                throw new UnauthorizedAccessException("User does not have Establishment role");
            }

            // Parse category
            if (!Enum.TryParse<EstablishmentCategory>(dto.Category, out var category))
            {
                throw new ArgumentException($"Invalid category: {dto.Category}");
            }

            var establishment = new Establishment
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = category,
                Address = dto.Address,
                City = "Baguio",
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                ContactNumber = dto.ContactNumber,
                Email = dto.Email,
                Website = dto.Website,
                FacebookPage = dto.FacebookPage,
                OwnerId = ownerId,
                Status = EstablishmentStatus.Pending,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Establishments.Add(establishment);
            await _context.SaveChangesAsync();

            // Add subcategories
            if (dto.SubcategoryIds != null && dto.SubcategoryIds.Any())
            {
                var subcategories = await _context
                    .EstablishmentSubcategories.Where(sc => dto.SubcategoryIds.Contains(sc.Id))
                    .ToListAsync();

                establishment.Subcategories = subcategories;
                await _context.SaveChangesAsync();
            }

            return await GetEstablishmentByIdAsync(establishment.Id)
                ?? throw new Exception("Failed to retrieve created establishment");
        }

        public async Task<EstablishmentDetailDto> UpdateEstablishmentAsync(
            int establishmentId,
            int ownerId,
            UpdateEstablishmentDto dto
        )
        {
            var establishment = await _context
                .Establishments.Include(e => e.Subcategories)
                .FirstOrDefaultAsync(e => e.Id == establishmentId && e.OwnerId == ownerId);

            if (establishment == null)
            {
                throw new UnauthorizedAccessException("Establishment not found or access denied");
            }

            // Parse category
            if (!Enum.TryParse<EstablishmentCategory>(dto.Category, out var category))
            {
                throw new ArgumentException($"Invalid category: {dto.Category}");
            }

            establishment.Name = dto.Name;
            establishment.Description = dto.Description;
            establishment.Category = category;
            establishment.Address = dto.Address;
            establishment.Latitude = dto.Latitude;
            establishment.Longitude = dto.Longitude;
            establishment.ContactNumber = dto.ContactNumber;
            establishment.Email = dto.Email;
            establishment.Website = dto.Website;
            establishment.FacebookPage = dto.FacebookPage;
            establishment.IsActive = dto.IsActive;
            establishment.UpdateTimestamp();

            // Update subcategories
            establishment.Subcategories.Clear();
            if (dto.SubcategoryIds != null && dto.SubcategoryIds.Any())
            {
                var subcategories = await _context
                    .EstablishmentSubcategories.Where(sc => dto.SubcategoryIds.Contains(sc.Id))
                    .ToListAsync();

                foreach (var subcategory in subcategories)
                {
                    establishment.Subcategories.Add(subcategory);
                }
            }

            await _context.SaveChangesAsync();

            return await GetEstablishmentByIdAsync(establishmentId)
                ?? throw new Exception("Failed to retrieve updated establishment");
        }

        public async Task<bool> DeleteEstablishmentAsync(int establishmentId, int ownerId)
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId && e.OwnerId == ownerId
            );

            if (establishment == null)
            {
                return false;
            }

            _context.Establishments.Remove(establishment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> UploadBusinessPermitAsync(
            int establishmentId,
            int ownerId,
            IFormFile file
        )
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId && e.OwnerId == ownerId
            );

            if (establishment == null)
            {
                throw new UnauthorizedAccessException("Establishment not found or access denied");
            }

            // Delete old permit if exists
            if (!string.IsNullOrEmpty(establishment.BusinessPermitS3Key))
            {
                await _s3Service.DeleteFileAsync(establishment.BusinessPermitS3Key);
            }

            // Upload new permit (use private upload for business documents)
            var folder = $"establishments/{establishmentId}/permits";
            var uploadResponse = await _s3Service.UploadPrivateFileAsync(file, folder);

            if (!uploadResponse.Success)
            {
                throw new Exception($"Failed to upload business permit: {uploadResponse.Error}");
            }

            establishment.BusinessPermitS3Key = uploadResponse.S3Key;
            establishment.UpdateTimestamp();

            await _context.SaveChangesAsync();

            // Generate presigned URL for immediate access
            return await _s3Service.GetPresignedUrlAsync(
                uploadResponse.S3Key,
                TimeSpan.FromHours(1)
            );
        }

        public async Task<string> UploadLogoAsync(int establishmentId, int ownerId, IFormFile file)
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId && e.OwnerId == ownerId
            );

            if (establishment == null)
            {
                throw new UnauthorizedAccessException("Establishment not found or access denied");
            }

            var folder = $"establishments/{establishmentId}/logo";
            var uploadResponse = await _s3Service.UploadFileAsync(file, folder);

            if (!uploadResponse.Success)
            {
                throw new Exception($"Failed to upload logo: {uploadResponse.Error}");
            }

            establishment.Logo = uploadResponse.FileUrl;
            establishment.UpdateTimestamp();

            await _context.SaveChangesAsync();

            return uploadResponse.FileUrl;
        }

        public async Task<string> UploadCoverImageAsync(
            int establishmentId,
            int ownerId,
            IFormFile file
        )
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId && e.OwnerId == ownerId
            );

            if (establishment == null)
            {
                throw new UnauthorizedAccessException("Establishment not found or access denied");
            }

            var folder = $"establishments/{establishmentId}/cover";
            var uploadResponse = await _s3Service.UploadFileAsync(file, folder);

            if (!uploadResponse.Success)
            {
                throw new Exception($"Failed to upload cover image: {uploadResponse.Error}");
            }

            establishment.CoverImage = uploadResponse.FileUrl;
            establishment.UpdateTimestamp();

            await _context.SaveChangesAsync();

            return uploadResponse.FileUrl;
        }

        public async Task<EstablishmentImageDto> AddEstablishmentImageAsync(
            int establishmentId,
            int ownerId,
            IFormFile file,
            string? caption,
            int displayOrder
        )
        {
            var establishment = await _context
                .Establishments.Include(e => e.Images)
                .FirstOrDefaultAsync(e => e.Id == establishmentId && e.OwnerId == ownerId);

            if (establishment == null)
            {
                throw new UnauthorizedAccessException("Establishment not found or access denied");
            }

            var folder = $"establishments/{establishmentId}/gallery";
            var uploadResponse = await _s3Service.UploadFileAsync(file, folder);

            if (!uploadResponse.Success)
            {
                throw new Exception($"Failed to upload image: {uploadResponse.Error}");
            }

            var image = new EstablishmentImage
            {
                EstablishmentId = establishmentId,
                ImageUrl = uploadResponse.FileUrl,
                S3Key = uploadResponse.S3Key,
                Caption = caption,
                DisplayOrder = displayOrder,
                CreatedAt = DateTime.UtcNow,
            };

            _context.EstablishmentImages.Add(image);
            await _context.SaveChangesAsync();

            return new EstablishmentImageDto
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl,
                DisplayOrder = image.DisplayOrder,
                Caption = image.Caption,
            };
        }

        public async Task<bool> DeleteEstablishmentImageAsync(
            int establishmentId,
            int imageId,
            int ownerId
        )
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId && e.OwnerId == ownerId
            );

            if (establishment == null)
            {
                return false;
            }

            var image = await _context.EstablishmentImages.FirstOrDefaultAsync(i =>
                i.Id == imageId && i.EstablishmentId == establishmentId
            );

            if (image == null)
            {
                return false;
            }

            // Delete from S3
            await _s3Service.DeleteFileAsync(image.S3Key);

            _context.EstablishmentImages.Remove(image);
            await _context.SaveChangesAsync();

            return true;
        }

        // ==================== MENU MANAGEMENT ====================

        public async Task<EstablishmentMenuItemDto> AddMenuItemAsync(
            int establishmentId,
            int ownerId,
            CreateMenuItemDto dto
        )
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId && e.OwnerId == ownerId
            );

            if (establishment == null)
            {
                throw new UnauthorizedAccessException("Establishment not found or access denied");
            }

            var menuItem = new EstablishmentMenuItem
            {
                EstablishmentId = establishmentId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category,
                IsAvailable = dto.IsAvailable,
                DisplayOrder = dto.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.EstablishmentMenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            return new EstablishmentMenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                Category = menuItem.Category,
                ImageUrl = menuItem.ImageUrl,
                IsAvailable = menuItem.IsAvailable,
                DisplayOrder = menuItem.DisplayOrder,
            };
        }

        public async Task<EstablishmentMenuItemDto> UpdateMenuItemAsync(
            int establishmentId,
            int menuItemId,
            int ownerId,
            UpdateMenuItemDto dto
        )
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId && e.OwnerId == ownerId
            );

            if (establishment == null)
            {
                throw new UnauthorizedAccessException("Establishment not found or access denied");
            }

            var menuItem = await _context.EstablishmentMenuItems.FirstOrDefaultAsync(m =>
                m.Id == menuItemId && m.EstablishmentId == establishmentId
            );

            if (menuItem == null)
            {
                throw new KeyNotFoundException("Menu item not found");
            }

            menuItem.Name = dto.Name;
            menuItem.Description = dto.Description;
            menuItem.Price = dto.Price;
            menuItem.Category = dto.Category;
            menuItem.IsAvailable = dto.IsAvailable;
            menuItem.DisplayOrder = dto.DisplayOrder;
            menuItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new EstablishmentMenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                Category = menuItem.Category,
                ImageUrl = menuItem.ImageUrl,
                IsAvailable = menuItem.IsAvailable,
                DisplayOrder = menuItem.DisplayOrder,
            };
        }

        public async Task<bool> DeleteMenuItemAsync(
            int establishmentId,
            int menuItemId,
            int ownerId
        )
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId && e.OwnerId == ownerId
            );

            if (establishment == null)
            {
                return false;
            }

            var menuItem = await _context.EstablishmentMenuItems.FirstOrDefaultAsync(m =>
                m.Id == menuItemId && m.EstablishmentId == establishmentId
            );

            if (menuItem == null)
            {
                return false;
            }

            _context.EstablishmentMenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> UploadMenuItemImageAsync(
            int establishmentId,
            int menuItemId,
            int ownerId,
            IFormFile file
        )
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId && e.OwnerId == ownerId
            );

            if (establishment == null)
            {
                throw new UnauthorizedAccessException("Establishment not found or access denied");
            }

            var menuItem = await _context.EstablishmentMenuItems.FirstOrDefaultAsync(m =>
                m.Id == menuItemId && m.EstablishmentId == establishmentId
            );

            if (menuItem == null)
            {
                throw new KeyNotFoundException("Menu item not found");
            }

            var folder = $"establishments/{establishmentId}/menu";
            var uploadResponse = await _s3Service.UploadFileAsync(file, folder);

            if (!uploadResponse.Success)
            {
                throw new Exception($"Failed to upload menu item image: {uploadResponse.Error}");
            }

            menuItem.ImageUrl = uploadResponse.FileUrl;
            menuItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return uploadResponse.FileUrl;
        }

        // ==================== ADMIN ENDPOINTS ====================

        public async Task<List<AdminEstablishmentListDto>> GetPendingEstablishmentsAsync()
        {
            var establishments = await _context
                .Establishments.Include(e => e.Owner)
                .Include(e => e.Subcategories)
                .Where(e => e.Status == EstablishmentStatus.Pending)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

            var result = new List<AdminEstablishmentListDto>();
            foreach (var e in establishments)
            {
                result.Add(
                    new AdminEstablishmentListDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Category = e.Category.ToString(),
                        Subcategories = e.Subcategories.Select(sc => sc.Name).ToList(),
                        OwnerName = $"{e.Owner.FirstName} {e.Owner.LastName}",
                        OwnerEmail = e.Owner.Email,
                        Status = e.Status.ToString(),
                        IsActive = e.IsActive,
                        BusinessPermitUrl = !string.IsNullOrEmpty(e.BusinessPermitS3Key)
                            ? await _s3Service.GetPresignedUrlAsync(
                                e.BusinessPermitS3Key,
                                TimeSpan.FromHours(1)
                            )
                            : null,
                        CreatedAt = e.CreatedAt,
                        ApprovedAt = e.ApprovedAt,
                        RejectionReason = e.RejectionReason,
                    }
                );
            }

            return result;
        }

        public async Task<List<AdminEstablishmentListDto>> GetAllEstablishmentsAsync(
            string? status,
            string? category
        )
        {
            var query = _context
                .Establishments.Include(e => e.Owner)
                .Include(e => e.Subcategories)
                .Include(e => e.ApprovedBy)
                .AsQueryable();

            if (
                !string.IsNullOrEmpty(status)
                && Enum.TryParse<EstablishmentStatus>(status, out var statusEnum)
            )
            {
                query = query.Where(e => e.Status == statusEnum);
            }

            if (
                !string.IsNullOrEmpty(category)
                && Enum.TryParse<EstablishmentCategory>(category, out var categoryEnum)
            )
            {
                query = query.Where(e => e.Category == categoryEnum);
            }

            var establishments = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();

            var result = new List<AdminEstablishmentListDto>();
            foreach (var e in establishments)
            {
                result.Add(
                    new AdminEstablishmentListDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Category = e.Category.ToString(),
                        Subcategories = e.Subcategories.Select(sc => sc.Name).ToList(),
                        OwnerName = $"{e.Owner.FirstName} {e.Owner.LastName}",
                        OwnerEmail = e.Owner.Email,
                        Status = e.Status.ToString(),
                        IsActive = e.IsActive,
                        BusinessPermitUrl = !string.IsNullOrEmpty(e.BusinessPermitS3Key)
                            ? await _s3Service.GetPresignedUrlAsync(
                                e.BusinessPermitS3Key,
                                TimeSpan.FromHours(1)
                            )
                            : null,
                        CreatedAt = e.CreatedAt,
                        ApprovedAt = e.ApprovedAt,
                        ApprovedByName =
                            e.ApprovedBy != null
                                ? $"{e.ApprovedBy.FirstName} {e.ApprovedBy.LastName}"
                                : null,
                        RejectionReason = e.RejectionReason,
                    }
                );
            }

            return result;
        }

        public async Task<bool> ApproveEstablishmentAsync(int establishmentId, int adminId)
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId
            );

            if (establishment == null)
            {
                return false;
            }

            establishment.Approve(adminId);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectEstablishmentAsync(
            int establishmentId,
            int adminId,
            string rejectionReason
        )
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId
            );

            if (establishment == null)
            {
                return false;
            }

            establishment.Reject(rejectionReason);
            establishment.ApprovedById = adminId;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string?> GetBusinessPermitDownloadUrlAsync(int establishmentId)
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e =>
                e.Id == establishmentId
            );

            if (establishment == null || string.IsNullOrEmpty(establishment.BusinessPermitS3Key))
            {
                return null;
            }

            return await _s3Service.GetPresignedUrlAsync(
                establishment.BusinessPermitS3Key,
                TimeSpan.FromHours(1)
            );
        }

        // ==================== HELPER METHODS ====================

        private static int GetDayOrder(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => 1,
                DayOfWeek.Tuesday => 2,
                DayOfWeek.Wednesday => 3,
                DayOfWeek.Thursday => 4,
                DayOfWeek.Friday => 5,
                DayOfWeek.Saturday => 6,
                DayOfWeek.Sunday => 7,
                _ => 8,
            };
        }
    }
}
