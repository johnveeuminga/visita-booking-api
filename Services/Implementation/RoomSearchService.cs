using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;
using System.Text.Json;
using AutoMapper;
using VisitaBookingApi.Data;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;

namespace visita_booking_api.Services.Implementation
{
    public class RoomSearchService : IRoomSearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoomCalendarService _calendarService;
        private readonly visita_booking_api.Services.Interfaces.IAvailabilityLedgerService _ledgerService;
        private readonly IRoomPriceCacheService _priceCacheService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<RoomSearchService> _logger;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        // Cache keys and TTL
        private readonly string _cachePrefix = "search:";
        private readonly TimeSpan _searchResultsTTL;
        private readonly TimeSpan _roomInventoryTTL;
        private readonly TimeSpan _suggestionsTTL;

        public RoomSearchService(
            ApplicationDbContext context,
            IRoomCalendarService calendarService,
            IRoomPriceCacheService priceCacheService,
            IDistributedCache cache,
            ILogger<RoomSearchService> logger,
            IMapper mapper,
            IConfiguration configuration,
            visita_booking_api.Services.Interfaces.IAvailabilityLedgerService ledgerService)
        {
            _context = context;
            _calendarService = calendarService;
            _ledgerService = ledgerService;
            _priceCacheService = priceCacheService;
            _cache = cache;
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;

            // Configure cache TTL from settings
            _searchResultsTTL = TimeSpan.FromMinutes(configuration.GetValue<int>("Caching:SearchResultsTTLMinutes", 3));
            _roomInventoryTTL = TimeSpan.FromMinutes(configuration.GetValue<int>("Caching:AvailabilityTTLMinutes", 5));
            _suggestionsTTL = TimeSpan.FromHours(configuration.GetValue<int>("Caching:AmenitiesTTLHours", 1));
        }

        public async Task<RoomSearchResponseDTO> SearchRoomsAsync(RoomSearchRequestDTO searchRequest)
        {
            var stopwatch = Stopwatch.StartNew();
            var searchId = Guid.NewGuid().ToString("N")[..8];

            try
            {
                _logger.LogInformation("Starting optimized room search {SearchId} for {Guests} guests from {CheckIn} to {CheckOut} with accommodation filter {AccommodationId}", 
                    searchId, searchRequest.Guests, searchRequest.CheckInDate, searchRequest.CheckOutDate, searchRequest.AccommodationId.HasValue ? searchRequest.AccommodationId.Value.ToString() : "None");

                // Generate cache key for this search
                var cacheKey = GenerateSearchCacheKey(searchRequest);
                
                // Try to get cached results first
                var cachedResult = await GetCachedSearchResultAsync(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Cache hit for search {SearchId}", searchId);
                    cachedResult.Metadata.CacheHit = true;
                    cachedResult.Metadata.SearchId = searchId;
                    cachedResult.Metadata.SearchDuration = stopwatch.Elapsed;
                    return cachedResult;
                }

                // Validate search parameters
                ValidateSearchRequestAsync(searchRequest);

                // PHASE 1: Ultra-fast price range EXCLUSION filtering using cached data
                // This eliminates 80-90% of rooms in milliseconds by finding what to EXCLUDE
                List<int>? excludedRoomIds = null;
                if (searchRequest.MinPrice.HasValue || searchRequest.MaxPrice.HasValue)
                {
                    _logger.LogDebug("Applying price cache exclusion filtering for search {SearchId}", searchId);
                    
                    // Use the proper exclusion method
                    excludedRoomIds = await _priceCacheService.GetRoomIdsToExcludeByPriceRangeAsync(
                        searchRequest.MinPrice,
                        searchRequest.MaxPrice,
                        searchRequest.CheckInDate,
                        searchRequest.CheckOutDate);
                    
                    _logger.LogDebug("Price cache exclusion filtering for search {SearchId}: {ExcludedCount} rooms to exclude", 
                        searchId, excludedRoomIds.Count);
                }

                // PHASE 2: Fast availability EXCLUSION filtering - prefer ledger for candidate rooms
                _logger.LogDebug("Applying availability exclusion filtering (ledger-first) for search {SearchId}", searchId);

                // Build a quick candidate set using cheap DB filters (accommodation + guests + price exclusion)
                var candidateQuery = _context.Rooms.Where(r => r.IsActive);
                if (excludedRoomIds != null && excludedRoomIds.Any())
                    candidateQuery = candidateQuery.Where(r => !excludedRoomIds.Contains(r.Id));
                if (searchRequest.Guests > 0)
                    candidateQuery = candidateQuery.Where(r => r.MaxGuests >= searchRequest.Guests);
                if (searchRequest.AccommodationId.HasValue)
                    candidateQuery = candidateQuery.Where(r => r.AccommodationId == searchRequest.AccommodationId.Value);

                // Get candidate ids (limit to a reasonable cap to avoid huge HMGET storms)
                var candidateIds = await candidateQuery.Select(r => r.Id).Take(5000).ToListAsync();

                // Try to get availability from ledger for candidates
                var ledgerMap = await _ledgerService.TryGetMinAvailableFromLedgerAsync(candidateIds, searchRequest.CheckInDate, searchRequest.CheckOutDate);

                // Rooms that ledger says have minAvailable <=0 are unavailable
                var unavailableFromLedger = ledgerMap.Where(kv => kv.Value <= 0).Select(kv => kv.Key).ToList();

                // Rooms missing from ledger need calendar-based exclusion (slower); ask calendar for all missing rooms
                var missingLedgerIds = candidateIds.Except(ledgerMap.Keys).ToList();
                var unavailableFromCalendar = new List<int>();
                if (missingLedgerIds.Any())
                {
                    // Let calendar compute min available units for missing rooms and mark those below requested quantity as unavailable
                    var fallback = await _calendarService.GetMinAvailableUnitsForRoomsAsync(missingLedgerIds, searchRequest.CheckInDate, searchRequest.CheckOutDate);
                    var requiredUnits = Math.Max(1, searchRequest.Quantity);
                    unavailableFromCalendar = fallback.Where(kv => kv.Value < requiredUnits).Select(kv => kv.Key).ToList();
                }

                var unavailableRoomIds = new HashSet<int>(unavailableFromLedger);
                foreach (var id in unavailableFromCalendar) unavailableRoomIds.Add(id);

                _logger.LogDebug("Availability exclusion (ledger-first) for search {SearchId}: {UnavailableCount} unavailable rooms to exclude", searchId, unavailableRoomIds.Count);

                // PHASE 3: Apply remaining filters with EXCLUSION approach at database level
                var filteredQuery = _context.Rooms.Where(r => r.IsActive);

                // Exclude rooms that don't meet price criteria (if price filter applied)
                if (excludedRoomIds != null && excludedRoomIds.Any())
                {
                    filteredQuery = filteredQuery.Where(r => !excludedRoomIds.Contains(r.Id));
                }

                // Exclude unavailable rooms
                if (unavailableRoomIds.Any())
                {
                    filteredQuery = filteredQuery.Where(r => !unavailableRoomIds.Contains(r.Id));
                }

                // Apply positive filters (these are still inclusion-based as they're more efficient this way)
                if (searchRequest.Guests > 0)
                {
                    filteredQuery = filteredQuery.Where(r => r.MaxGuests >= searchRequest.Guests);
                }

                // PRIORITY FILTER: Filter by accommodation ID to dramatically reduce search scope
                if (searchRequest.AccommodationId.HasValue)
                {
                    filteredQuery = filteredQuery.Where(r => r.AccommodationId == searchRequest.AccommodationId.Value);
                }

                // Amenity filters
                if (searchRequest.RequiredAmenities.Any())
                {
                    foreach (var amenityId in searchRequest.RequiredAmenities)
                    {
                        filteredQuery = filteredQuery.Where(r => r.RoomAmenities.Any(ra => ra.AmenityId == amenityId));
                    }
                }

                if (searchRequest.AmenityCategories.Any())
                {
                    filteredQuery = filteredQuery.Where(r => r.RoomAmenities
                        .Any(ra => searchRequest.AmenityCategories.Contains(ra.Amenity.Category)));
                }

                // Text search
                if (!string.IsNullOrWhiteSpace(searchRequest.SearchTerm))
                {
                    var searchTerm = searchRequest.SearchTerm.ToLower();
                    filteredQuery = filteredQuery.Where(r => 
                        r.Name.ToLower().Contains(searchTerm) || 
                        r.Description.ToLower().Contains(searchTerm));
                }

                // Other filters
                if (searchRequest.HasPhotos == true)
                {
                    filteredQuery = filteredQuery.Where(r => r.Photos.Any(p => p.IsActive));
                }

                if (searchRequest.CreatedAfter.HasValue)
                {
                    filteredQuery = filteredQuery.Where(r => r.CreatedAt >= searchRequest.CreatedAfter);
                }
                if (searchRequest.UpdatedAfter.HasValue)
                {
                    filteredQuery = filteredQuery.Where(r => r.UpdatedAt >= searchRequest.UpdatedAfter);
                }

                // Get total count before pagination
                var totalCount = await filteredQuery.CountAsync();

                if (totalCount == 0)
                {
                    return CreateEmptySearchResponse(searchRequest, searchId, stopwatch.Elapsed);
                }

                // PHASE 4: Apply sorting and pagination to the highly filtered set
                var sortedQuery = ApplySortingAndPagination(filteredQuery, searchRequest);

                // PHASE 5: Execute the final query with optimized includes
                var rooms = await sortedQuery
                    .Include(r => r.Photos.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).Take(5))
                    .Include(r => r.RoomAmenities.Take(10))
                        .ThenInclude(ra => ra.Amenity)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogDebug("Database query for search {SearchId}: retrieved {Count} rooms for final processing", 
                    searchId, rooms.Count);

                // PHASE 6: Calculate exact pricing only for final results (10-50 rooms vs 10,000+)
                var roomIds = rooms.Select(r => r.Id).ToList();
                var roomPrices = await _calendarService.GetRoomPricesAsync(roomIds, searchRequest.CheckInDate, searchRequest.CheckOutDate);

                // Get daily price breakdown for detailed pricing info
                var dailyPrices = await GetDailyPricesForRoomsAsync(roomIds, searchRequest.CheckInDate, searchRequest.CheckOutDate);

                // PHASE 7: Build search results with exact pricing
                var searchResults = await BuildSearchResultsAsync(rooms, roomPrices, dailyPrices, searchRequest);

                // Calculate metadata
                var metadata = BuildSmartPaginationMetadata(totalCount, searchResults.Count, searchRequest, searchId, stopwatch.Elapsed, roomPrices.Values);

                var response = new RoomSearchResponseDTO
                {
                    Results = searchResults,
                    Metadata = metadata,
                    AppliedFilters = BuildAppliedFilters(searchRequest)
                };

                // Cache the results
                await CacheSearchResultAsync(cacheKey, response);

                _logger.LogInformation("Completed optimized search {SearchId} in {Duration}ms: {Results} results from {Total} total matches", 
                    searchId, stopwatch.ElapsedMilliseconds, searchResults.Count, totalCount);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in optimized room search {SearchId}", searchId);
                throw;
            }
        }

        /// <summary>
        /// Execute smart pagination search that fetches multiple batches to ensure we get the requested page size
        /// even after price filtering
        /// </summary>
        private async Task<List<Room>> ExecuteSmartPaginationSearchAsync(RoomSearchRequestDTO searchRequest, string searchId)
        {
            var results = new List<Room>();
            var fetchedPages = 0;
            var maxFetchPages = 10; // Prevent infinite loops - max 10 batches
            var hasMinPriceFilter = searchRequest.MinPrice.HasValue;
            var hasMaxPriceFilter = searchRequest.MaxPrice.HasValue;
            var hasPriceFilter = hasMinPriceFilter || hasMaxPriceFilter;
            
            _logger.LogDebug("Starting smart pagination for search {SearchId}, price filter: {HasPriceFilter}", searchId, hasPriceFilter);

            // Step 1: Get unavailable room IDs once
            var unavailableRoomIds = await GetUnavailableRoomIdsWithCachingAsync(
                searchRequest.CheckInDate, 
                searchRequest.CheckOutDate);

            // Step 2: Build base query without pagination
            var baseQuery = BuildFilteredRoomQuery(searchRequest, unavailableRoomIds);
            
            // If no price filter, we can use simple pagination
            if (!hasPriceFilter)
            {
                var simpleQuery = ApplySortingAndPagination(baseQuery, searchRequest);
                return await simpleQuery
                    .Include(r => r.Photos.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).Take(5))
                    .Include(r => r.RoomAmenities.Take(10))
                        .ThenInclude(ra => ra.Amenity)
                    .AsNoTracking()
                    .ToListAsync();
            }

            // Step 3: Smart pagination with price filtering
            var currentSkip = (searchRequest.Page - 1) * searchRequest.PageSize;
            
            while (results.Count < searchRequest.PageSize && fetchedPages < maxFetchPages)
            {
                // Calculate batch size - increase size for later batches to find more matches
                var batchMultiplier = Math.Min(3 + fetchedPages, 5); // Start at 3x, max 5x
                var batchSize = searchRequest.PageSize * batchMultiplier;
                
                _logger.LogDebug("Fetching batch {BatchNumber} for search {SearchId}, skip: {Skip}, take: {Take}", 
                    fetchedPages + 1, searchId, currentSkip, batchSize);

                // Fetch next batch of rooms with includes
                var batchQuery = baseQuery
                    .Skip(currentSkip)
                    .Take(batchSize);
                    
                var batchRooms = await batchQuery
                    .Include(r => r.Photos.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).Take(5))
                    .Include(r => r.RoomAmenities.Take(10))
                        .ThenInclude(ra => ra.Amenity)
                    .AsNoTracking()
                    .ToListAsync();
                
                if (!batchRooms.Any())
                {
                    _logger.LogDebug("No more rooms found in batch {BatchNumber} for search {SearchId}", fetchedPages + 1, searchId);
                    break;
                }
                
                // Apply price filtering to this batch
                var validRoomsFromBatch = await FilterRoomsByPriceAsync(batchRooms, searchRequest, searchId);
                // Further filter by inventory (available units >= required units)
                var requiredUnits = Math.Max(1, searchRequest.Quantity); // Use requested quantity from DTO
                var batchIds = validRoomsFromBatch.Select(r => r.Id).ToList();
                // Prefer ledger data for fast reads
                var availableMap = await _ledgerService.TryGetMinAvailableFromLedgerAsync(batchIds, searchRequest.CheckInDate, searchRequest.CheckOutDate);
                // For rooms missing ledger data, fallback to calendar computation
                var missing = batchIds.Except(availableMap.Keys).ToList();
                if (missing.Any())
                {
                    var fallback = await _calendarService.GetMinAvailableUnitsForRoomsAsync(missing, searchRequest.CheckInDate, searchRequest.CheckOutDate);
                    foreach (var kv in fallback) availableMap[kv.Key] = kv.Value;
                }
                var inventoryFiltered = validRoomsFromBatch.Where(r => availableMap.GetValueOrDefault(r.Id, 0) >= requiredUnits).ToList();

                _logger.LogDebug("Inventory filtering for search {SearchId}: batch {BatchNumber} -> {InventoryFilteredCount} rooms", searchId, fetchedPages + 1, inventoryFiltered.Count);

                results.AddRange(inventoryFiltered);
                
                // Update skip position for next batch
                currentSkip += batchSize;
                fetchedPages++;
                
                _logger.LogDebug("Batch {BatchNumber} for search {SearchId}: fetched {FetchedCount}, valid after price filter: {ValidCount}, total results: {TotalResults}", 
                    fetchedPages, searchId, batchRooms.Count, validRoomsFromBatch.Count, results.Count);
            }

            // Step 4: Apply final sorting to the combined results
            var sortedResults = ApplyFinalSorting(results, searchRequest);
            
            _logger.LogInformation("Smart pagination complete for search {SearchId}: {TotalBatches} batches, {TotalResults} results found", 
                searchId, fetchedPages, sortedResults.Count);
            
            return sortedResults;
        }

        /// <summary>
        /// Filter rooms by price after fetching calendar pricing data
        /// </summary>
        private async Task<List<Room>> FilterRoomsByPriceAsync(List<Room> rooms, RoomSearchRequestDTO searchRequest, string searchId)
        {
            if (!searchRequest.MinPrice.HasValue && !searchRequest.MaxPrice.HasValue)
            {
                return rooms;
            }

            try
            {
                // Get pricing data for this batch
                var roomIds = rooms.Select(r => r.Id).ToList();
                var roomPrices = await _calendarService.GetRoomPricesAsync(roomIds, searchRequest.CheckInDate, searchRequest.CheckOutDate);
                var nights = (searchRequest.CheckOutDate - searchRequest.CheckInDate).Days;

                var validRooms = rooms.Where(room =>
                {
                    var totalPrice = roomPrices.GetValueOrDefault(room.Id, room.DefaultPrice * nights);
                    var nightlyAverage = nights > 0 ? totalPrice / nights : totalPrice;

                    // Apply min price filter
                    if (searchRequest.MinPrice.HasValue && nightlyAverage < searchRequest.MinPrice)
                        return false;
                        
                    // Apply max price filter
                    if (searchRequest.MaxPrice.HasValue && nightlyAverage > searchRequest.MaxPrice)
                        return false;

                    return true;
                }).ToList();

                _logger.LogDebug("Price filtering for search {SearchId}: {InputCount} rooms -> {OutputCount} rooms (min: {MinPrice}, max: {MaxPrice})", 
                    searchId, rooms.Count, validRooms.Count, searchRequest.MinPrice, searchRequest.MaxPrice);

                return validRooms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering rooms by price for search {SearchId}", searchId);
                // Return original rooms if pricing fails to avoid complete search failure
                return rooms;
            }
        }

        /// <summary>
        /// Apply final sorting to the combined results from multiple batches
        /// </summary>
        private List<Room> ApplyFinalSorting(List<Room> rooms, RoomSearchRequestDTO searchRequest)
        {
            var query = rooms.AsQueryable();
            
            // Apply sorting (same logic as ApplySortingAndPagination but without pagination)
            query = searchRequest.SortBy switch
            {
                RoomSearchSortBy.Price => searchRequest.SortOrder == SortOrder.Ascending 
                    ? query.OrderBy(r => r.DefaultPrice) 
                    : query.OrderByDescending(r => r.DefaultPrice),
                RoomSearchSortBy.Name => searchRequest.SortOrder == SortOrder.Ascending 
                    ? query.OrderBy(r => r.Name) 
                    : query.OrderByDescending(r => r.Name),
                RoomSearchSortBy.CreatedDate => searchRequest.SortOrder == SortOrder.Ascending 
                    ? query.OrderBy(r => r.CreatedAt) 
                    : query.OrderByDescending(r => r.CreatedAt),
                RoomSearchSortBy.UpdatedDate => searchRequest.SortOrder == SortOrder.Ascending 
                    ? query.OrderBy(r => r.UpdatedAt) 
                    : query.OrderByDescending(r => r.UpdatedAt),
                RoomSearchSortBy.Popularity => query.OrderByDescending(r => r.Id), // Placeholder for popularity
                _ => query.OrderBy(r => r.Name) // Default: Name
            };

            return query.ToList();
        }

        /// <summary>
        /// Build metadata specifically for smart pagination results
        /// </summary>
        private SearchMetadataDTO BuildSmartPaginationMetadata(
            int totalFoundResults,
            int returnedResults,
            RoomSearchRequestDTO searchRequest,
            string searchId,
            TimeSpan searchDuration,
            IEnumerable<decimal> prices)
        {
            var priceList = prices.ToList();

            return new SearchMetadataDTO
            {
                TotalResults = returnedResults, // For smart pagination, this is the actual returned count
                TotalPages = 1, // Smart pagination doesn't use traditional paging
                CurrentPage = searchRequest.Page,
                PageSize = searchRequest.PageSize,
                HasNextPage = totalFoundResults > searchRequest.PageSize, // More results were found than requested
                HasPreviousPage = searchRequest.Page > 1,
                SearchDuration = searchDuration,
                CacheHit = false,
                SearchId = searchId,
                MinPrice = priceList.Any() ? priceList.Min() : null,
                MaxPrice = priceList.Any() ? priceList.Max() : null,
                AveragePrice = priceList.Any() ? priceList.Average() : null,
                AvailableRooms = returnedResults,
                UnavailableRooms = 0,
                IsSmartPagination = true,
                ActualResultsFound = totalFoundResults
            };
        }

        public async Task<List<QuickSearchResultDTO>> QuickSearchAsync(QuickSearchRequestDTO searchRequest)
        {
            var cacheKey = $"{_cachePrefix}quick:{searchRequest.SearchTerm}:{searchRequest.CheckInDate?.ToString("yyyy-MM-dd")}:{searchRequest.Limit}";
            
            var cachedResults = await GetCachedDataAsync<List<QuickSearchResultDTO>>(cacheKey);
            if (cachedResults != null)
            {
                return cachedResults;
            }

            var query = _context.Rooms
                .Where(r => r.IsActive)
                .AsQueryable();

            // Apply text search if provided
            if (!string.IsNullOrWhiteSpace(searchRequest.SearchTerm))
            {
                var searchTerm = searchRequest.SearchTerm.ToLower();
                query = query.Where(r => 
                    r.Name.ToLower().Contains(searchTerm) || 
                    r.Description.ToLower().Contains(searchTerm));
            }

            // Get availability if dates provided
            List<int>? availableRoomIds = null;
            if (searchRequest.CheckInDate.HasValue && searchRequest.CheckOutDate.HasValue)
            {
                availableRoomIds = await _calendarService.GetAvailableRoomIdsAsync(
                    searchRequest.CheckInDate.Value,
                    searchRequest.CheckOutDate.Value);
                
                if (availableRoomIds.Any())
                {
                    query = query.Where(r => availableRoomIds.Contains(r.Id));
                }
            }

            var results = await query
                .OrderBy(r => r.Name)
                .Take(searchRequest.Limit)
                .AsNoTracking()
                .ToListAsync();

            var quickResults = results.Select(r => new QuickSearchResultDTO
            {
                Id = r.Id,
                Name = r.Name,
                DefaultPrice = r.DefaultPrice,
                MainPhotoUrl = r.Photos.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).FirstOrDefault()?.CdnUrl,
                AmenityCount = r.RoomAmenities.Count,
                IsAvailable = availableRoomIds == null || availableRoomIds.Contains(r.Id)
            }).ToList();

            await SetCacheAsync(cacheKey, quickResults, _suggestionsTTL);
            return quickResults;
        }

        public async Task<SearchSuggestionResponseDTO> GetSearchSuggestionsAsync(SearchSuggestionRequestDTO request)
        {
            var cacheKey = $"{_cachePrefix}suggestions:{request.Query}:{request.Limit}";
            
            var cachedSuggestions = await GetCachedDataAsync<SearchSuggestionResponseDTO>(cacheKey);
            if (cachedSuggestions != null)
            {
                return cachedSuggestions;
            }

            var query = request.Query.ToLower();

            // Get room name suggestions
            var roomNames = await _context.Rooms
                .Where(r => r.IsActive && r.Name.ToLower().Contains(query))
                .OrderBy(r => r.Name)
                .Take(request.Limit)
                .Select(r => r.Name)
                .Distinct()
                .ToListAsync();

            // Get amenity suggestions
            var amenities = await _context.Amenities
                .Where(a => a.IsActive && a.Name.ToLower().Contains(query))
                .OrderBy(a => a.Name)
                .Take(request.Limit)
                .Select(a => a.Name)
                .Distinct()
                .ToListAsync();

            // Get popular price ranges
            var popularPriceRanges = await GetPopularPriceRangesAsync();

            var suggestions = new SearchSuggestionResponseDTO
            {
                RoomNames = roomNames,
                Amenities = amenities,
                Categories = Enum.GetNames<AmenityCategory>().Take(request.Limit).ToList(),
                PopularPriceRanges = popularPriceRanges.Take(request.Limit).ToList()
            };

            await SetCacheAsync(cacheKey, suggestions, _suggestionsTTL);
            return suggestions;
        }

        public async Task<List<decimal>> GetPopularPriceRangesAsync()
        {
            var cacheKey = $"{_cachePrefix}price-ranges";
            
            var cachedRanges = await GetCachedDataAsync<List<decimal>>(cacheKey);
            if (cachedRanges != null)
            {
                return cachedRanges;
            }

            // Get price distribution and create popular ranges
            var prices = await _context.Rooms
                .Where(r => r.IsActive)
                .Select(r => r.DefaultPrice)
                .ToListAsync();

            var ranges = new List<decimal>();
            if (prices.Any())
            {
                var min = prices.Min();
                var max = prices.Max();
                var range = (max - min) / 5; // Create 5 price brackets

                for (int i = 0; i < 5; i++)
                {
                    ranges.Add(min + (range * i));
                }
            }

            await SetCacheAsync(cacheKey, ranges, TimeSpan.FromHours(24));
            return ranges;
        }

        public async Task<Dictionary<string, object>> GetSearchStatsAsync(RoomSearchRequestDTO searchRequest)
        {
            var availableRoomIds = await _calendarService.GetAvailableRoomIdsAsync(
                searchRequest.CheckInDate, 
                searchRequest.CheckOutDate);

            var totalRooms = await _context.Rooms.CountAsync(r => r.IsActive);
            
            return new Dictionary<string, object>
            {
                ["TotalActiveRooms"] = totalRooms,
                ["AvailableRooms"] = availableRoomIds.Count,
                ["UnavailableRooms"] = totalRooms - availableRoomIds.Count,
                ["OccupancyRate"] = totalRooms > 0 ? (double)(totalRooms - availableRoomIds.Count) / totalRooms * 100 : 0,
                ["SearchDate"] = DateTime.UtcNow
            };
        }

        #region Private Helper Methods

        private async Task<List<int>> GetUnavailableRoomIdsWithCachingAsync(
            DateTime checkIn, 
            DateTime checkOut)
        {
            var dateKey = $"{checkIn:yyyy-MM-dd}:{checkOut:yyyy-MM-dd}";
            var cacheKey = $"{_cachePrefix}unavailable:{dateKey}";

            var cachedUnavailable = await GetCachedDataAsync<List<int>>(cacheKey);
            if (cachedUnavailable != null)
            {
                return cachedUnavailable;
            }

            // Get all active room IDs
            var allActiveRoomIds = await _context.Rooms
                .Where(r => r.IsActive)
                .Select(r => r.Id)
                .ToListAsync();

            // Get available room IDs
            var availableRoomIds = await _calendarService.GetAvailableRoomIdsAsync(checkIn, checkOut);

            // Calculate unavailable room IDs (all active rooms minus available ones)
            var unavailableRoomIds = allActiveRoomIds.Except(availableRoomIds).ToList();

            await SetCacheAsync(cacheKey, unavailableRoomIds, _roomInventoryTTL);
            return unavailableRoomIds;
        }

        private async Task<IQueryable<Room>> BuildFilteredRoomQueryAsync(
            RoomSearchRequestDTO searchRequest, 
            List<int> unavailableRoomIds)
        {
            return await Task.FromResult(BuildFilteredRoomQuery(searchRequest, unavailableRoomIds));
        }

        private IQueryable<Room> BuildFilteredRoomQuery(
            RoomSearchRequestDTO searchRequest, 
            List<int> unavailableRoomIds)
        {
            var query = _context.Rooms
                .Where(r => r.IsActive && !unavailableRoomIds.Contains(r.Id))
                .AsQueryable();

            // PRIORITY FILTER: Filter by accommodation ID first to dramatically reduce search scope
            if (searchRequest.AccommodationId.HasValue)
            {
                query = query.Where(r => r.AccommodationId == searchRequest.AccommodationId.Value);
            }

            // Filter by guest capacity
            if (searchRequest.Guests > 0)
            {
                query = query.Where(r => r.MaxGuests >= searchRequest.Guests);
            }

            // Filter by amenities
            if (searchRequest.RequiredAmenities.Any())
            {
                foreach (var amenityId in searchRequest.RequiredAmenities)
                {
                    query = query.Where(r => r.RoomAmenities.Any(ra => ra.AmenityId == amenityId));
                }
            }

            // Filter by amenity categories
            if (searchRequest.AmenityCategories.Any())
            {
                query = query.Where(r => r.RoomAmenities
                    .Any(ra => searchRequest.AmenityCategories.Contains(ra.Amenity.Category)));
            }

            // Text search
            if (!string.IsNullOrWhiteSpace(searchRequest.SearchTerm))
            {
                var searchTerm = searchRequest.SearchTerm.ToLower();
                query = query.Where(r => 
                    r.Name.ToLower().Contains(searchTerm) || 
                    r.Description.ToLower().Contains(searchTerm));
            }

            // Price range filtering using default price (rough filter)
            if (searchRequest.MinPrice.HasValue)
            {
                query = query.Where(r => r.DefaultPrice >= searchRequest.MinPrice);
            }
            if (searchRequest.MaxPrice.HasValue)
            {
                query = query.Where(r => r.DefaultPrice <= searchRequest.MaxPrice);
            }

            // Has photos filter
            if (searchRequest.HasPhotos == true)
            {
                query = query.Where(r => r.Photos.Any(p => p.IsActive));
            }

            // Date filters
            if (searchRequest.CreatedAfter.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= searchRequest.CreatedAfter);
            }
            if (searchRequest.UpdatedAfter.HasValue)
            {
                query = query.Where(r => r.UpdatedAt >= searchRequest.UpdatedAfter);
            }

            return query;
        }

        private async Task<List<Room>> ApplyPriceRangeFilterAsync(
            List<Room> rooms, 
            Dictionary<int, decimal> roomPrices, 
            RoomSearchRequestDTO searchRequest)
        {
            if (!searchRequest.MinPrice.HasValue && !searchRequest.MaxPrice.HasValue)
            {
                return rooms;
            }

            var nights = (searchRequest.CheckOutDate - searchRequest.CheckInDate).Days;
            
            return await Task.FromResult(rooms.Where(room =>
            {
                var totalPrice = roomPrices.GetValueOrDefault(room.Id, 0);
                var dailyAverage = nights > 0 ? totalPrice / nights : totalPrice;

                if (searchRequest.MinPrice.HasValue && dailyAverage < searchRequest.MinPrice)
                    return false;
                    
                if (searchRequest.MaxPrice.HasValue && dailyAverage > searchRequest.MaxPrice)
                    return false;

                return true;
            }).ToList());
        }

        private IQueryable<Room> ApplySortingAndPagination(
            IQueryable<Room> query, 
            RoomSearchRequestDTO searchRequest)
        {
            // Apply sorting
            query = searchRequest.SortBy switch
            {
                RoomSearchSortBy.Price => searchRequest.SortOrder == SortOrder.Ascending 
                    ? query.OrderBy(r => r.DefaultPrice) 
                    : query.OrderByDescending(r => r.DefaultPrice),
                RoomSearchSortBy.Name => searchRequest.SortOrder == SortOrder.Ascending 
                    ? query.OrderBy(r => r.Name) 
                    : query.OrderByDescending(r => r.Name),
                RoomSearchSortBy.CreatedDate => searchRequest.SortOrder == SortOrder.Ascending 
                    ? query.OrderBy(r => r.CreatedAt) 
                    : query.OrderByDescending(r => r.CreatedAt),
                RoomSearchSortBy.UpdatedDate => searchRequest.SortOrder == SortOrder.Ascending 
                    ? query.OrderBy(r => r.UpdatedAt) 
                    : query.OrderByDescending(r => r.UpdatedAt),
                RoomSearchSortBy.Popularity => query.OrderByDescending(r => r.Id), // Placeholder for popularity
                _ => query.OrderBy(r => r.Name) // Default: Relevance/Name
            };

            // Apply pagination
            return query.Skip((searchRequest.Page - 1) * searchRequest.PageSize)
                        .Take(searchRequest.PageSize);
        }

        private async Task<Dictionary<int, List<DailyPriceDTO>>> GetDailyPricesForRoomsAsync(
            List<int> roomIds, 
            DateTime checkIn, 
            DateTime checkOut)
        {
            var dailyPrices = new Dictionary<int, List<DailyPriceDTO>>();
            
            foreach (var roomId in roomIds)
            {
                var roomDailyPrices = new List<DailyPriceDTO>();
                
                for (var date = checkIn; date < checkOut; date = date.AddDays(1))
                {
                    var price = await _calendarService.GetPriceForDateAsync(roomId, date);
                    var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                    var isHoliday = await _calendarService.IsHolidayAsync(date);

                    roomDailyPrices.Add(new DailyPriceDTO
                    {
                        Date = date,
                        Price = price,
                        IsWeekend = isWeekend,
                        IsHoliday = isHoliday,
                        PriceNote = isHoliday ? "Holiday Rate" : isWeekend ? "Weekend Rate" : null
                    });
                }
                
                dailyPrices[roomId] = roomDailyPrices;
            }

            return dailyPrices;
        }

        private async Task<List<RoomSearchResultDTO>> BuildSearchResultsAsync(
            List<Room> rooms, 
            Dictionary<int, decimal> roomPrices,
            Dictionary<int, List<DailyPriceDTO>> dailyPrices,
            RoomSearchRequestDTO searchRequest)
        {
            var results = new List<RoomSearchResultDTO>();

            // Bulk compute available units for rooms across the date range
            var roomIds = rooms.Select(r => r.Id).ToList();
            var availableUnitsMap = await _ledgerService.TryGetMinAvailableFromLedgerAsync(roomIds, searchRequest.CheckInDate, searchRequest.CheckOutDate);
            var missingFinal = roomIds.Except(availableUnitsMap.Keys).ToList();
            if (missingFinal.Any())
            {
                var fallbackFinal = await _calendarService.GetMinAvailableUnitsForRoomsAsync(missingFinal, searchRequest.CheckInDate, searchRequest.CheckOutDate);
                foreach (var kv in fallbackFinal) availableUnitsMap[kv.Key] = kv.Value;
            }

            foreach (var room in rooms)
            {
                var totalPrice = roomPrices.GetValueOrDefault(room.Id, 0);
                var nights = (searchRequest.CheckOutDate - searchRequest.CheckInDate).Days;
                var averagePrice = nights > 0 ? totalPrice / nights : room.DefaultPrice;

                // Apply price range filter on calculated prices if needed
                if (searchRequest.MinPrice.HasValue && totalPrice < searchRequest.MinPrice * nights) continue;
                if (searchRequest.MaxPrice.HasValue && totalPrice > searchRequest.MaxPrice * nights) continue;

                var result = new RoomSearchResultDTO
                {
                    Id = room.Id,
                    Name = room.Name,
                    Description = room.Description,
                    DefaultPrice = room.DefaultPrice,
                    MaxGuests = room.MaxGuests,
                    CalculatedPrice = averagePrice,
                    TotalPrice = totalPrice,
                    MainPhotoUrl = room.MainPhotoUrl,
                    PhotoUrls = room.Photos.Where(p => p.IsActive)
                        .OrderBy(p => p.DisplayOrder)
                        .Select(p => p.CdnUrl ?? p.S3Url)
                        .ToList(),
                    TopAmenities = room.RoomAmenities.Take(5)
                        .Select(ra => new AmenityDTO
                        {
                            Id = ra.Amenity.Id,
                            Name = ra.Amenity.Name,
                            Description = ra.Amenity.Description,
                            Category = ra.Amenity.Category,
                            IsActive = ra.Amenity.IsActive,
                            LastModified = ra.Amenity.CreatedAt,
                            DisplayOrder = ra.Amenity.DisplayOrder
                        }).ToList(),
                    TotalAmenities = room.RoomAmenities.Count,
                    AvailableUnits = availableUnitsMap.GetValueOrDefault(room.Id, 0),
                    IsAvailable = availableUnitsMap.GetValueOrDefault(room.Id, 0) > 0,
                    AvailabilityStatus = availableUnitsMap.GetValueOrDefault(room.Id, 0) > 0 ? "Available" : "Unavailable",
                    RelevanceScore = CalculateRelevanceScore(room, searchRequest),
                    PopularityScore = 0, // Placeholder
                    LastUpdated = room.UpdatedAt,
                    DailyPrices = dailyPrices.GetValueOrDefault(room.Id, new List<DailyPriceDTO>())
                };

                results.Add(result);
            }

            return results;
        }

        private float CalculateRelevanceScore(Room room, RoomSearchRequestDTO searchRequest)
        {
            float score = 1.0f;

            // Boost score for exact guest capacity match
            if (room.MaxGuests == searchRequest.Guests)
                score += 0.5f;
            else if (room.MaxGuests >= searchRequest.Guests)
                score += 0.2f;

            // Boost score for matching amenities
            if (searchRequest.RequiredAmenities.Any())
            {
                var matchingAmenities = room.RoomAmenities
                    .Count(ra => searchRequest.RequiredAmenities.Contains(ra.AmenityId));
                score += (float)matchingAmenities / searchRequest.RequiredAmenities.Count * 0.3f;
            }

            // Boost score for text match in name
            if (!string.IsNullOrWhiteSpace(searchRequest.SearchTerm) && 
                room.Name.Contains(searchRequest.SearchTerm, StringComparison.OrdinalIgnoreCase))
            {
                score += 0.4f;
            }

            return score;
        }

        private SearchMetadataDTO BuildSearchMetadataAsync(
            int totalResults,
            RoomSearchRequestDTO searchRequest,
            string searchId,
            TimeSpan searchDuration,
            IEnumerable<decimal> prices)
        {
            var totalPages = (int)Math.Ceiling((double)totalResults / searchRequest.PageSize);
            var priceList = prices.ToList();

            return new SearchMetadataDTO
            {
                TotalResults = totalResults,
                TotalPages = totalPages,
                CurrentPage = searchRequest.Page,
                PageSize = searchRequest.PageSize,
                HasNextPage = searchRequest.Page < totalPages,
                HasPreviousPage = searchRequest.Page > 1,
                SearchDuration = searchDuration,
                CacheHit = false,
                SearchId = searchId,
                MinPrice = priceList.Any() ? priceList.Min() : null,
                MaxPrice = priceList.Any() ? priceList.Max() : null,
                AveragePrice = priceList.Any() ? priceList.Average() : null,
                AvailableRooms = totalResults,
                UnavailableRooms = 0 // Will be calculated if needed
            };
        }

        private SearchFiltersDTO BuildAppliedFilters(RoomSearchRequestDTO searchRequest)
        {
            return new SearchFiltersDTO
            {
                CheckInDate = searchRequest.CheckInDate,
                CheckOutDate = searchRequest.CheckOutDate,
                Guests = searchRequest.Guests,
                MinPrice = searchRequest.MinPrice,
                MaxPrice = searchRequest.MaxPrice,
                RequiredAmenities = new List<AmenityDTO>(), // Will be populated if needed
                AmenityCategories = searchRequest.AmenityCategories,
                SearchTerm = searchRequest.SearchTerm,
                AccommodationId = searchRequest.AccommodationId,
                SortBy = searchRequest.SortBy,
                SortOrder = searchRequest.SortOrder
            };
        }

        private RoomSearchResponseDTO CreateEmptySearchResponse(
            RoomSearchRequestDTO searchRequest, 
            string searchId, 
            TimeSpan duration)
        {
            return new RoomSearchResponseDTO
            {
                Results = new List<RoomSearchResultDTO>(),
                Metadata = new SearchMetadataDTO
                {
                    TotalResults = 0,
                    TotalPages = 0,
                    CurrentPage = searchRequest.Page,
                    PageSize = searchRequest.PageSize,
                    HasNextPage = false,
                    HasPreviousPage = false,
                    SearchDuration = duration,
                    CacheHit = false,
                    SearchId = searchId,
                    AvailableRooms = 0,
                    UnavailableRooms = 0
                },
                AppliedFilters = BuildAppliedFilters(searchRequest)
            };
        }

        private void ValidateSearchRequestAsync(RoomSearchRequestDTO searchRequest)
        {
            if (searchRequest.CheckOutDate <= searchRequest.CheckInDate)
                throw new ArgumentException("Check-out date must be after check-in date");

            if (searchRequest.Page < 1)
                throw new ArgumentException("Page must be greater than 0");

            if (searchRequest.PageSize < 1 || searchRequest.PageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100");

            if (searchRequest.Guests < 1)
                throw new ArgumentException("Number of guests must be at least 1");
        }

        #endregion

        #region Cache Helper Methods

        private string GenerateSearchCacheKey(RoomSearchRequestDTO request)
        {
            var keyData = new
            {
                request.CheckInDate,
                request.CheckOutDate,
                request.Guests,
                request.MinPrice,
                request.MaxPrice,
                RequiredAmenities = string.Join(",", request.RequiredAmenities.OrderBy(x => x)),
                AmenityCategories = string.Join(",", request.AmenityCategories.OrderBy(x => x)),
                request.SearchTerm,
                request.AccommodationId,  // Include accommodation ID in cache key
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortOrder,
                request.HasPhotos
            };

            var keyJson = JsonSerializer.Serialize(keyData);
            var keyHash = keyJson.GetHashCode().ToString();
            return $"{_cachePrefix}search:{keyHash}";
        }

        private async Task<RoomSearchResponseDTO?> GetCachedSearchResultAsync(string cacheKey)
        {
            return await GetCachedDataAsync<RoomSearchResponseDTO>(cacheKey);
        }

        private async Task CacheSearchResultAsync(string cacheKey, RoomSearchResponseDTO response)
        {
            await SetCacheAsync(cacheKey, response, _searchResultsTTL);
        }

        private async Task<T?> GetCachedDataAsync<T>(string cacheKey) where T : class
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<T>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving cache for key: {CacheKey}", cacheKey);
            }
            return null;
        }

        private async Task SetCacheAsync<T>(string cacheKey, T data, TimeSpan expiration)
        {
            try
            {
                var serializedData = JsonSerializer.Serialize(data);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };
                await _cache.SetStringAsync(cacheKey, serializedData, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting cache for key: {CacheKey}", cacheKey);
            }
        }

        #endregion
    }
}