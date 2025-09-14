using visita_booking_api.Models.DTOs;

namespace visita_booking_api.Services.Interfaces
{
    public interface IAccommodationSearchService
    {
        /// <summary>
        /// Search accommodations with available rooms for the specified criteria
        /// </summary>
        Task<AccommodationSearchResponseDTO> SearchAccommodationsAsync(AccommodationSearchRequestDTO searchRequest);

        /// <summary>
        /// Get quick accommodation suggestions for auto-complete
        /// </summary>
        Task<List<AccommodationQuickSearchResultDTO>> QuickSearchAccommodationsAsync(string searchTerm, int limit = 10);

        /// <summary>
        /// Get search suggestions for accommodation search
        /// </summary>
        Task<AccommodationSearchSuggestionResponseDTO> GetAccommodationSearchSuggestionsAsync(string query, int limit = 10);

        /// <summary>
        /// Warm up cache for popular accommodations
        /// </summary>
        Task WarmUpAccommodationCacheAsync();
    }

    // Additional DTOs for accommodation search
    public class AccommodationQuickSearchResultDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Logo { get; set; }
        public int AvailableRooms { get; set; }
        public decimal? LowestPrice { get; set; }
        public List<string> TopAmenities { get; set; } = new();
    }

    public class AccommodationSearchSuggestionResponseDTO
    {
        public List<string> AccommodationNames { get; set; } = new();
        public List<string> PopularAmenities { get; set; } = new();
        public List<decimal> PopularPriceRanges { get; set; } = new();
        public List<string> Locations { get; set; } = new();
    }
}