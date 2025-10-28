// using System.ComponentModel.DataAnnotations;
// using visita_booking_api.Models.Entities;

// namespace visita_booking_api.Models.DTOs
// {
//     // Accommodation-based Search Response DTOs
//     public class AccommodationSearchResponseDTO
//     {
//         public List<AccommodationSearchResultDTO> Results { get; set; } = new();
//         public SearchMetadataDTO Metadata { get; set; } = new();
//         public SearchFiltersDTO AppliedFilters { get; set; } = new();
//         public AccommodationSearchStatsDTO Stats { get; set; } = new();
//     }

//     public class AccommodationSearchResultDTO
//     {
//         // Accommodation Details
//         public int Id { get; set; }
//         public string Name { get; set; } = string.Empty;
//         public string? Description { get; set; }
//         public string? Logo { get; set; }
//         public string? Address { get; set; }
//         public string? EmailAddress { get; set; }
//         public string? ContactNo { get; set; }
//         public bool IsActive { get; set; }

//         // Available Rooms in this Accommodation
//         public List<AvailableRoomDTO> AvailableRooms { get; set; } = new();
//         public int TotalAvailableRooms { get; set; }
//         public int TotalRoomsInAccommodation { get; set; }

//         // Pricing Information
//         public decimal? LowestRoomPrice { get; set; }
//         public decimal? HighestRoomPrice { get; set; }
//         public decimal? AverageRoomPrice { get; set; }
//         public decimal? TotalStayPrice { get; set; } // Cheapest room's total stay cost

//         // Amenities (aggregated from all rooms)
//         public List<string> PopularAmenities { get; set; } = new(); // Top amenities across all rooms
//         public int TotalUniqueAmenities { get; set; }

//         // Search-specific fields
//         public float RelevanceScore { get; set; }
//         public int PopularityScore { get; set; }
//         public DateTime LastUpdated { get; set; }

//         // Accommodation Images (if any)
//         public string? MainImageUrl { get; set; }
//         public List<string> ImageUrls { get; set; } = new();
//     }

//     public class AvailableRoomDTO
//     {
//         public int Id { get; set; }
//         public string Name { get; set; } = string.Empty;
//         public string Description { get; set; } = string.Empty;
//         public decimal DefaultPrice { get; set; }
//         public int MaxGuests { get; set; }

//         // Pricing for the search period
//         public decimal? CalculatedPrice { get; set; } // Per night for searched dates
//         public decimal? TotalPrice { get; set; } // Total price for the stay duration

//         // Room Images
//         public string? MainPhotoUrl { get; set; }
//         public List<string> PhotoUrls { get; set; } = new();

//         // Top amenities for this room
//         public List<AmenityDTO> TopAmenities { get; set; } = new();
//         public int TotalAmenities { get; set; }

//         // Availability status
//         public bool IsAvailable { get; set; }
//         public string AvailabilityStatus { get; set; } = string.Empty;

//         // Pricing breakdown (optional, for detailed view)
//         public List<DailyPriceDTO> DailyPrices { get; set; } = new();

//         // Room-specific popularity/relevance
//         public float RoomRelevanceScore { get; set; }
//         public DateTime LastUpdated { get; set; }
//     }

//     public class AccommodationSearchStatsDTO
//     {
//         public int TotalAccommodations { get; set; }
//         public int AccommodationsWithAvailability { get; set; }
//         public int TotalAvailableRooms { get; set; }
//         public int TotalSearchedRooms { get; set; }

//         // Price statistics across all accommodations
//         public decimal? GlobalMinPrice { get; set; }
//         public decimal? GlobalMaxPrice { get; set; }
//         public decimal? GlobalAveragePrice { get; set; }

//         // Availability distribution
//         public Dictionary<string, int> AvailabilityDistribution { get; set; } = new();
//         // e.g., {"1-2 rooms": 5, "3-5 rooms": 8, "6+ rooms": 2}
//     }

//     // Enhanced Search Request for Accommodation-based search
//     public class AccommodationSearchRequestDTO
//     {
//         [Required]
//         public DateTime CheckInDate { get; set; }

//         [Required]
//         public DateTime CheckOutDate { get; set; }

//         [Range(1, 9999)]
//         public int Guests { get; set; } = 1;

//         [Range(1, 100)]
//         public int Quantity { get; set; } = 1; // Number of units requested per room

//         // Pagination
//         public int Page { get; set; } = 1;
//         public int PageSize { get; set; } = 10; // Accommodations per page

//         // Filters
//         public decimal? MinPrice { get; set; } // Per room per night
//         public decimal? MaxPrice { get; set; } // Per room per night
//         public decimal? MinTotalPrice { get; set; } // Total stay price
//         public decimal? MaxTotalPrice { get; set; } // Total stay price

//         public List<int> RequiredAmenityIds { get; set; } = new();
//         public List<AmenityCategory> AmenityCategories { get; set; } = new();

//         // Search term (accommodation name, description, location)
//         public string? SearchTerm { get; set; }

//         // Sorting
//         public AccommodationSearchSortBy SortBy { get; set; } = AccommodationSearchSortBy.Relevance;
//         public SortOrder SortOrder { get; set; } = SortOrder.Descending;

//         // Room constraints
//         public int? MinRoomsAvailable { get; set; } // Minimum number of available rooms
//         public bool GroupSimilarRooms { get; set; } = true; // Group rooms with same features

//         // Advanced filters
//         public List<int>? SpecificAccommodationIds { get; set; } // Filter to specific accommodations
//         public bool IncludeUnavailableAccommodations { get; set; } = false; // Show accommodations even if no rooms available

//         public int? SearchSessionSeed { get; set; } // For consistent pagination with randomized results
// }

//     // Sorting options for accommodation search
//     public enum AccommodationSearchSortBy
//     {
//         Relevance,
//         Price, // Lowest room price
//         Name,
//         AvailableRooms, // Number of available rooms
//         Popularity,
//         LastUpdated,
//         TotalStayPrice // Total cost for the stay
//     }
// }

using System.ComponentModel.DataAnnotations;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Models.DTOs
{
    // Accommodation-based Search Response DTOs
    public class AccommodationSearchResponseDTO
    {
        public List<AccommodationSearchResultDTO> Results { get; set; } = new();
        public SearchMetadataDTO Metadata { get; set; } = new();
        public SearchFiltersDTO AppliedFilters { get; set; } = new();
        public AccommodationSearchStatsDTO Stats { get; set; } = new();
    }

    public class AccommodationSearchResultDTO
    {
        // Accommodation Details
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Logo { get; set; }
        public string? Address { get; set; }
        public string? EmailAddress { get; set; }
        public string? ContactNo { get; set; }
        public bool IsActive { get; set; }

        // Available Rooms in this Accommodation
        public List<AvailableRoomDTO> AvailableRooms { get; set; } = new();
        public int TotalAvailableRooms { get; set; }
        public int TotalRoomsInAccommodation { get; set; }

        // Pricing Information
        public decimal? LowestRoomPrice { get; set; }
        public decimal? HighestRoomPrice { get; set; }
        public decimal? AverageRoomPrice { get; set; }
        public decimal? TotalStayPrice { get; set; } // Cheapest room's total stay cost

        // Amenities (aggregated from all rooms)
        public List<string> PopularAmenities { get; set; } = new(); // Top amenities across all rooms
        public int TotalUniqueAmenities { get; set; }

        // Search-specific fields
        public float RelevanceScore { get; set; }
        public int PopularityScore { get; set; }
        public DateTime LastUpdated { get; set; }

        // Accommodation Images (if any)
        public string? MainImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }

    public class AvailableRoomDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
        public int MaxGuests { get; set; }

        // Pricing for the search period
        public decimal? CalculatedPrice { get; set; } // Per night for searched dates
        public decimal? TotalPrice { get; set; } // Total price for the stay duration

        // Room Images
        public string? MainPhotoUrl { get; set; }
        public List<string> PhotoUrls { get; set; } = new();

        // Top amenities for this room
        public List<AmenityDTO> TopAmenities { get; set; } = new();
        public int TotalAmenities { get; set; }

        // Availability status
        public bool IsAvailable { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;

        // Pricing breakdown (optional, for detailed view)
        public List<DailyPriceDTO> DailyPrices { get; set; } = new();

        // Room-specific popularity/relevance
        public float RoomRelevanceScore { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class AccommodationSearchStatsDTO
    {
        public int TotalAccommodations { get; set; }
        public int AccommodationsWithAvailability { get; set; }
        public int TotalAvailableRooms { get; set; }
        public int TotalSearchedRooms { get; set; }

        // Price statistics across all accommodations
        public decimal? GlobalMinPrice { get; set; }
        public decimal? GlobalMaxPrice { get; set; }
        public decimal? GlobalAveragePrice { get; set; }

        // Availability distribution
        public Dictionary<string, int> AvailabilityDistribution { get; set; } = new();
        // e.g., {"1-2 rooms": 5, "3-5 rooms": 8, "6+ rooms": 2}
    }

    // Enhanced Search Request for Accommodation-based search
    public class AccommodationSearchRequestDTO
    {
        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 9999)]
        public int Guests { get; set; } = 1;

        [Range(1, 100)]
        public int Quantity { get; set; } = 1; // Number of units requested per room

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10; // Accommodations per page

        // Filters
        public decimal? MinPrice { get; set; } // Per room per night
        public decimal? MaxPrice { get; set; } // Per room per night
        public decimal? MinTotalPrice { get; set; } // Total stay price
        public decimal? MaxTotalPrice { get; set; } // Total stay price

        public List<int> RequiredAmenityIds { get; set; } = new();
        public List<AmenityCategory> AmenityCategories { get; set; } = new();

        // Search term (accommodation name, description, location)
        public string? SearchTerm { get; set; }

        // Sorting
        public AccommodationSearchSortBy SortBy { get; set; } = AccommodationSearchSortBy.Relevance;
        public SortOrder SortOrder { get; set; } = SortOrder.Descending;

        // Room constraints
        public int? MinRoomsAvailable { get; set; } // Minimum number of available rooms
        public bool GroupSimilarRooms { get; set; } = true; // Group rooms with same features

        // Advanced filters
        public List<int>? SpecificAccommodationIds { get; set; } // Filter to specific accommodations
        public bool IncludeUnavailableAccommodations { get; set; } = false; // Show accommodations even if no rooms available
    }

    // Sorting options for accommodation search
    public enum AccommodationSearchSortBy
    {
        Relevance,
        Price, // Lowest room price
        Name,
        AvailableRooms, // Number of available rooms
        Popularity,
        LastUpdated,
        TotalStayPrice // Total cost for the stay
    }
}