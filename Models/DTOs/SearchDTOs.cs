using System.ComponentModel.DataAnnotations;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Models.DTOs
{
    // Search Request DTOs
    public class RoomSearchRequestDTO
    {
        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public int Guests { get; set; } = 1;
    [Range(1, 100)]
    public int Quantity { get; set; } = 1; // Number of units requested (for multi-unit bookings)

        [Range(0, 99999)]
        public decimal? MinPrice { get; set; }

        [Range(0, 99999)]
        public decimal? MaxPrice { get; set; }

        public List<int> RequiredAmenities { get; set; } = new();

        public List<AmenityCategory> AmenityCategories { get; set; } = new();

        public string? SearchTerm { get; set; }

        // Accommodation filter - limits search to rooms within specific accommodation
        public int? AccommodationId { get; set; }

        [Range(1, 100)]
        public int Page { get; set; } = 1;

        [Range(1, 50)]
        public int PageSize { get; set; } = 10;

        public RoomSearchSortBy SortBy { get; set; } = RoomSearchSortBy.Relevance;
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

        // Advanced filters
        public bool? HasPhotos { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? UpdatedAfter { get; set; }
    }

    public enum RoomSearchSortBy
    {
        Relevance = 0,
        Price = 1,
        Name = 2,
        CreatedDate = 3,
        UpdatedDate = 4,
        Popularity = 5
    }

    public enum SortOrder
    {
        Ascending = 0,
        Descending = 1
    }

    // Search Response DTOs
    public class RoomSearchResponseDTO
    {
        public List<RoomSearchResultDTO> Results { get; set; } = new();
        public SearchMetadataDTO Metadata { get; set; } = new();
        public SearchFiltersDTO AppliedFilters { get; set; } = new();
    }

    public class RoomSearchResultDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
        public int MaxGuests { get; set; }
        public decimal? CalculatedPrice { get; set; } // Price for the searched dates
        public decimal? TotalPrice { get; set; } // Total price for the stay duration

        public string? MainPhotoUrl { get; set; }
        public List<string> PhotoUrls { get; set; } = new();

        public List<AmenityDTO> TopAmenities { get; set; } = new(); // Top 5 amenities
        public int TotalAmenities { get; set; }

    // Inventory-aware availability
    public int AvailableUnits { get; set; }
    public bool IsAvailable { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;

        // Search-specific fields
        public float RelevanceScore { get; set; }
        public int PopularityScore { get; set; }
        public DateTime LastUpdated { get; set; }

        // Pricing breakdown
        public List<DailyPriceDTO> DailyPrices { get; set; } = new();
    }

    public class DailyPriceDTO
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public bool IsWeekend { get; set; }
        public bool IsHoliday { get; set; }
        public string? PriceNote { get; set; }
    }

    public class SearchMetadataDTO
    {
        public int TotalResults { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    public int ShuffleSeed { get; set; }

        public TimeSpan SearchDuration { get; set; }
        public bool CacheHit { get; set; }
        public string SearchId { get; set; } = string.Empty;

        // Price statistics
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? AveragePrice { get; set; }

        // Availability statistics
        public int AvailableRooms { get; set; }
        public int UnavailableRooms { get; set; }

        // Smart pagination properties
        public bool IsSmartPagination { get; set; }
        public int ActualResultsFound { get; set; }
    }

    public class PriceCacheStatsDTO
    {
        public int TotalCacheEntries { get; set; }
        public int ValidCacheEntries { get; set; }
        public int StaleCacheEntries { get; set; }
        public int ExpiredCacheEntries { get; set; }
        public DateTime? OldestCacheEntry { get; set; }
        public DateTime? NewestCacheEntry { get; set; }
        public double CacheHitRatio { get; set; }
        public int TotalSearchHits { get; set; }
        public decimal AverageMinPrice { get; set; }
        public decimal AverageMaxPrice { get; set; }
        public Dictionary<string, int> PriceBandDistribution { get; set; } = new();
        public DateTime LastRefreshTime { get; set; }
    }

    public class SearchFiltersDTO
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Guests { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<AmenityDTO> RequiredAmenities { get; set; } = new();
        public List<AmenityCategory> AmenityCategories { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? AccommodationId { get; set; }
        public RoomSearchSortBy SortBy { get; set; }
        public SortOrder SortOrder { get; set; }
    }

    // Quick Search DTOs
    public class QuickSearchRequestDTO
    {
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Limit { get; set; } = 5;
    }

    public class QuickSearchResultDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
        public string? MainPhotoUrl { get; set; }
        public int AmenityCount { get; set; }
        public bool IsAvailable { get; set; }
    }

    // Search Suggestions DTOs
    public class SearchSuggestionRequestDTO
    {
        public string Query { get; set; } = string.Empty;
        public int Limit { get; set; } = 10;
    }

    public class SearchSuggestionResponseDTO
    {
        public List<string> RoomNames { get; set; } = new();
        public List<string> Amenities { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<decimal> PopularPriceRanges { get; set; } = new();
    }
}