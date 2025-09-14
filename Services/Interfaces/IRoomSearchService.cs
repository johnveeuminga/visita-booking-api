using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Services.Interfaces
{
    public interface IRoomSearchService
    {
        /// <summary>
        /// Search for available rooms with filters and pagination
        /// </summary>
        Task<RoomSearchResponseDTO> SearchRoomsAsync(RoomSearchRequestDTO searchRequest);

        /// <summary>
        /// Quick search for rooms (simplified, faster)
        /// </summary>
        Task<List<QuickSearchResultDTO>> QuickSearchAsync(QuickSearchRequestDTO searchRequest);

        /// <summary>
        /// Get search suggestions for autocomplete
        /// </summary>
        Task<SearchSuggestionResponseDTO> GetSearchSuggestionsAsync(SearchSuggestionRequestDTO request);

        /// <summary>
        /// Get popular price ranges for filtering
        /// </summary>
        Task<List<decimal>> GetPopularPriceRangesAsync();

        /// <summary>
        /// Get room statistics for search metadata
        /// </summary>
        Task<Dictionary<string, object>> GetSearchStatsAsync(RoomSearchRequestDTO searchRequest);
    }
}