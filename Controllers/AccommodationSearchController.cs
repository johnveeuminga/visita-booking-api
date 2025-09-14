using Microsoft.AspNetCore.Mvc;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/accommodations")]
    public class AccommodationSearchController : ControllerBase
    {
        private readonly IAccommodationSearchService _searchService;
        private readonly ILogger<AccommodationSearchController> _logger;

        public AccommodationSearchController(
            IAccommodationSearchService searchService,
            ILogger<AccommodationSearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Search accommodations with available rooms for the specified criteria
        /// Returns paginated accommodations with their available rooms nested
        /// </summary>
        /// <param name="request">Search parameters including dates, guest count, price range, amenities, etc.</param>
        /// <returns>Paginated accommodation search results with metadata</returns>
        [HttpPost("search")]
        [ProducesResponseType(typeof(AccommodationSearchResponseDTO), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<AccommodationSearchResponseDTO>> SearchAccommodations([FromBody] AccommodationSearchRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var searchResults = await _searchService.SearchAccommodationsAsync(request);
                
                return Ok(searchResults);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid accommodation search request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing accommodation search");
                return StatusCode(500, new { error = "An error occurred while searching for accommodations" });
            }
        }

        /// <summary>
        /// Quick search for accommodation auto-complete and suggestions
        /// </summary>
        /// <param name="query">Search term</param>
        /// <param name="limit">Maximum number of results (default: 10, max: 20)</param>
        /// <returns>Quick search results</returns>
        [HttpGet("search/quick")]
        [ProducesResponseType(typeof(List<AccommodationQuickSearchResultDTO>), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<List<AccommodationQuickSearchResultDTO>>> QuickSearch(
            [FromQuery] string? query = null,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (limit < 1 || limit > 20)
                    return BadRequest(new { error = "Limit must be between 1 and 20" });

                var results = await _searchService.QuickSearchAccommodationsAsync(query ?? string.Empty, limit);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing quick accommodation search");
                return StatusCode(500, new { error = "An error occurred while performing quick search" });
            }
        }

        /// <summary>
        /// Get search suggestions for accommodation search auto-complete
        /// </summary>
        /// <param name="query">Partial search term</param>
        /// <param name="limit">Maximum number of suggestions (default: 10, max: 20)</param>
        /// <returns>Search suggestions</returns>
        [HttpGet("search/suggestions")]
        [ProducesResponseType(typeof(AccommodationSearchSuggestionResponseDTO), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<AccommodationSearchSuggestionResponseDTO>> GetSearchSuggestions(
            [FromQuery] string query,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return BadRequest(new { error = "Query parameter is required" });

                if (limit < 1 || limit > 20)
                    return BadRequest(new { error = "Limit must be between 1 and 20" });

                var suggestions = await _searchService.GetAccommodationSearchSuggestionsAsync(query, limit);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accommodation search suggestions");
                return StatusCode(500, new { error = "An error occurred while getting search suggestions" });
            }
        }

        /// <summary>
        /// Warm up accommodation search cache (admin endpoint)
        /// </summary>
        [HttpPost("search/cache/warmup")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> WarmUpCache()
        {
            try
            {
                await _searchService.WarmUpAccommodationCacheAsync();
                return Ok(new { message = "Cache warmup completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error warming up accommodation search cache");
                return StatusCode(500, new { error = "An error occurred while warming up cache" });
            }
        }
    }
}