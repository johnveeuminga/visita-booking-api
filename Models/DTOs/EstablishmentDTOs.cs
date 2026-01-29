using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VisitaBookingAPI.DTOs
{
    public class EstablishmentCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public List<EstablishmentSubcategoryDto> Subcategories { get; set; } = new();
    }

    public class EstablishmentSubcategoryDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class EstablishmentListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<string> Subcategories { get; set; } = new();
        public string? Logo { get; set; }
        public string? CoverImage { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? ContactNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EstablishmentDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<EstablishmentSubcategoryDto> Subcategories { get; set; } = new();
        public string? Logo { get; set; }
        public string? CoverImage { get; set; }
        public List<EstablishmentImageDto> Images { get; set; } = new();
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? FacebookPage { get; set; }
        public List<EstablishmentHoursDto> OperatingHours { get; set; } = new();
        public List<EstablishmentMenuItemDto> MenuItems { get; set; } = new();
        public EstablishmentOwnerDto Owner { get; set; } = null!;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CommentCount { get; set; }
        public double? AverageRating { get; set; }
    }

    public class EstablishmentImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string? Caption { get; set; }
    }

    public class EstablishmentHoursDto
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public bool IsClosed { get; set; }
    }

    public class EstablishmentMenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class EstablishmentOwnerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CreateEstablishmentDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        public List<int> SubcategoryIds { get; set; } = new();

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [Phone]
        public string? ContactNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Url]
        public string? Website { get; set; }

        public string? FacebookPage { get; set; }
    }

    public class UpdateEstablishmentDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        public List<int> SubcategoryIds { get; set; } = new();

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [Phone]
        public string? ContactNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Url]
        public string? Website { get; set; }

        public string? FacebookPage { get; set; }

        public bool IsActive { get; set; }
    }

    public class CreateMenuItemDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0, 999999.99)]
        public decimal? Price { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public bool IsAvailable { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateMenuItemDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0, 999999.99)]
        public decimal? Price { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public bool IsAvailable { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class RejectEstablishmentDto
    {
        [Required]
        public int EstablishmentId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class EstablishmentSearchParams
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public List<int>? SubcategoryIds { get; set; }
        public string? City { get; set; } = "Baguio";
        public bool? IsActive { get; set; } = true;
        public string? Status { get; set; } = "Approved";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "name";
        public string? SortOrder { get; set; } = "asc";
    }

    public class EstablishmentSearchResult
    {
        public List<EstablishmentListDto> Establishments { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminEstablishmentListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Subcategories { get; set; } = new();
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? BusinessPermitUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByName { get; set; }
        public string? RejectionReason { get; set; }
    }
}
