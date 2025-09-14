using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomSearchController : ControllerBase
    {
        private readonly IRoomSearchService _searchService;
        private readonly ILogger<RoomSearchController> _logger;

        public RoomSearchController(
            IRoomSearchService searchService,
            ILogger<RoomSearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Comprehensive room search with filters, smart pagination, and performance optimization
        /// </summary>
        /// <param name="request">Search parameters including dates, guest count, price range, amenities, etc.</param>
        /// <returns>Paginated search results with metadata</returns>
        [HttpPost("search")]
        [ProducesResponseType(typeof(RoomSearchResponseDTO), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<RoomSearchResponseDTO>> SearchRooms([FromBody] RoomSearchRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var searchResults = await _searchService.SearchRoomsAsync(request);
                
                return Ok(searchResults);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid search request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing room search");
                return StatusCode(500, new { error = "An error occurred while searching for rooms" });
            }
        }

        /// <summary>
        /// Quick search for auto-complete and suggestions
        /// </summary>
        /// <param name="query">Search term</param>
        /// <param name="checkIn">Optional check-in date for availability filtering</param>
        /// <param name="checkOut">Optional check-out date for availability filtering</param>
        /// <param name="limit">Maximum number of results (default: 10, max: 50)</param>
        /// <returns>Quick search results</returns>
        [HttpGet("search/quick")]
        [ProducesResponseType(typeof(List<QuickSearchResultDTO>), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<List<QuickSearchResultDTO>>> QuickSearch(
            [FromQuery] string? query = null,
            [FromQuery] DateTime? checkIn = null,
            [FromQuery] DateTime? checkOut = null,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (limit < 1 || limit > 50)
                    return BadRequest(new { error = "Limit must be between 1 and 50" });

                if (checkIn.HasValue && checkOut.HasValue && checkOut <= checkIn)
                    return BadRequest(new { error = "Check-out date must be after check-in date" });

                var searchRequest = new QuickSearchRequestDTO
                {
                    SearchTerm = query ?? string.Empty,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    Limit = limit
                };

                var results = await _searchService.QuickSearchAsync(searchRequest);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing quick search");
                return StatusCode(500, new { error = "An error occurred while performing quick search" });
            }
        }

        /// <summary>
        /// Get search suggestions for auto-complete
        /// </summary>
        /// <param name="query">Search term to get suggestions for</param>
        /// <param name="limit">Maximum number of suggestions (default: 10, max: 20)</param>
        /// <returns>Search suggestions including room names, amenities, categories</returns>
        [HttpGet("search/suggestions")]
        [ProducesResponseType(typeof(SearchSuggestionResponseDTO), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<SearchSuggestionResponseDTO>> GetSearchSuggestions(
            [FromQuery, Required] string query,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return BadRequest(new { error = "Query parameter is required" });

                if (limit < 1 || limit > 20)
                    return BadRequest(new { error = "Limit must be between 1 and 20" });

                var request = new SearchSuggestionRequestDTO
                {
                    Query = query.Trim(),
                    Limit = limit
                };

                var suggestions = await _searchService.GetSearchSuggestionsAsync(request);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions for query: {Query}", query);
                return StatusCode(500, new { error = "An error occurred while getting search suggestions" });
            }
        }

        /// <summary>
        /// Get popular price ranges for filtering
        /// </summary>
        /// <returns>List of popular price ranges</returns>
        [HttpGet("search/price-ranges")]
        [ProducesResponseType(typeof(List<decimal>), 200)]
        public async Task<ActionResult<List<decimal>>> GetPopularPriceRanges()
        {
            try
            {
                var priceRanges = await _searchService.GetPopularPriceRangesAsync();
                return Ok(priceRanges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular price ranges");
                return StatusCode(500, new { error = "An error occurred while getting price ranges" });
            }
        }

        /// <summary>
        /// Get search statistics for dashboard or analytics
        /// </summary>
        /// <param name="checkIn">Check-in date</param>
        /// <param name="checkOut">Check-out date</param>
        /// <param name="guests">Number of guests</param>
        /// <returns>Search statistics</returns>
        [HttpGet("search/stats")]
        [Authorize(Policy = "AdminPolicy")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<Dictionary<string, object>>> GetSearchStats(
            [FromQuery, Required] DateTime checkIn,
            [FromQuery, Required] DateTime checkOut,
            [FromQuery] int guests = 1)
        {
            try
            {
                if (checkOut <= checkIn)
                    return BadRequest(new { error = "Check-out date must be after check-in date" });

                if (guests < 1)
                    return BadRequest(new { error = "Number of guests must be at least 1" });

                var searchRequest = new RoomSearchRequestDTO
                {
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    Guests = guests,
                    Page = 1,
                    PageSize = 1 // We only need stats, not actual results
                };

                var stats = await _searchService.GetSearchStatsAsync(searchRequest);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search statistics");
                return StatusCode(500, new { error = "An error occurred while getting search statistics" });
            }
        }

        /// <summary>
        /// Test endpoint for smart pagination performance
        /// </summary>
        /// <param name="request">Search parameters</param>
        /// <returns>Search results with detailed performance metrics</returns>
        [HttpPost("search/test-performance")]
        [Authorize(Policy = "AdminPolicy")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ActionResult> TestSearchPerformance([FromBody] RoomSearchRequestDTO request)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                var results = await _searchService.SearchRoomsAsync(request);
                
                stopwatch.Stop();

                return Ok(new
                {
                    SearchResults = results,
                    PerformanceMetrics = new
                    {
                        TotalDuration = stopwatch.Elapsed,
                        RequestedPageSize = request.PageSize,
                        ActualResultsReturned = results.Results.Count,
                        IsSmartPagination = results.Metadata.IsSmartPagination,
                        ActualResultsFound = results.Metadata.ActualResultsFound,
                        CacheHit = results.Metadata.CacheHit,
                        SearchId = results.Metadata.SearchId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in search performance test");
                return StatusCode(500, new { error = "Performance test failed" });
            }
        }
    }
}