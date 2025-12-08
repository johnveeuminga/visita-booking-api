using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;

namespace visita_booking_api.Services.Implementation
{
    public class AccommodationSearchService : IAccommodationSearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoomCalendarService _calendarService;
        private readonly IAvailabilityLedgerService _ledgerService;
        private readonly IRoomPriceCacheService _priceCacheService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AccommodationSearchService> _logger;

        public AccommodationSearchService(
            ApplicationDbContext context,
            IRoomCalendarService calendarService,
            IRoomPriceCacheService priceCacheService,
            ICacheService cacheService,
            ILogger<AccommodationSearchService> logger,
            IAvailabilityLedgerService ledgerService
        )
        {
            _context = context;
            _calendarService = calendarService;
            _priceCacheService = priceCacheService;
            _cacheService = cacheService;
            _ledgerService = ledgerService;
            _logger = logger;
        }

        public async Task<AccommodationSearchResponseDTO> SearchAccommodationsAsync(
            AccommodationSearchRequestDTO searchRequest
        )
        {
            var stopwatch = Stopwatch.StartNew();
            var searchId = Guid.NewGuid().ToString("N")[..8];

            try
            {
                _logger.LogInformation(
                    "Starting optimized accommodation search {SearchId} for {Guests} guests from {CheckIn} to {CheckOut}",
                    searchId,
                    searchRequest.Guests,
                    searchRequest.CheckInDate,
                    searchRequest.CheckOutDate
                );

                // Generate cache key for this search
                var cacheKey = GenerateAccommodationSearchCacheKey(searchRequest);

                // Try to get cached results first
                var cachedResult = await GetCachedAccommodationSearchResultAsync(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation(
                        "Cache hit for accommodation search {SearchId}",
                        searchId
                    );
                    cachedResult.Metadata.CacheHit = true;
                    cachedResult.Metadata.SearchId = searchId;
                    cachedResult.Metadata.SearchDuration = stopwatch.Elapsed;
                    return cachedResult;
                }

                // Validate search parameters
                ValidateSearchRequest(searchRequest);

                // PHASE 1: Ultra-fast price range EXCLUSION filtering using cached data
                // This eliminates 80-90% of rooms in milliseconds by finding what to EXCLUDE
                List<int>? excludedRoomIds = null;
                if (searchRequest.MinPrice.HasValue || searchRequest.MaxPrice.HasValue)
                {
                    _logger.LogDebug(
                        "Applying price exclusion filtering for accommodation search {SearchId}",
                        searchId
                    );

                    // If dates are provided, use price cache for dynamic pricing
                    if (searchRequest.CheckInDate.HasValue && searchRequest.CheckOutDate.HasValue)
                    {
                        excludedRoomIds =
                            await _priceCacheService.GetRoomIdsToExcludeByPriceRangeAsync(
                                searchRequest.MinPrice,
                                searchRequest.MaxPrice,
                                searchRequest.CheckInDate.Value,
                                searchRequest.CheckOutDate.Value
                            );
                    }
                    else
                    {
                        // No dates provided - filter by DefaultPrice instead
                        var excludedByMin = new List<int>();
                        var excludedByMax = new List<int>();

                        if (searchRequest.MinPrice.HasValue)
                        {
                            excludedByMin = await _context
                                .Rooms.Where(r => r.DefaultPrice < searchRequest.MinPrice.Value)
                                .Select(r => r.Id)
                                .ToListAsync();
                        }

                        if (searchRequest.MaxPrice.HasValue)
                        {
                            excludedByMax = await _context
                                .Rooms.Where(r => r.DefaultPrice > searchRequest.MaxPrice.Value)
                                .Select(r => r.Id)
                                .ToListAsync();
                        }

                        // Combine both exclusion lists (OR logic)
                        excludedRoomIds = excludedByMin.Concat(excludedByMax).Distinct().ToList();
                    }

                    _logger.LogDebug(
                        "Price exclusion filtering for accommodation search {SearchId}: {ExcludedCount} rooms to exclude",
                        searchId,
                        excludedRoomIds?.Count ?? 0
                    );
                }
                // PHASE 2: Fast availability EXCLUSION filtering - prefer ledger for candidate rooms
                _logger.LogDebug(
                    "Applying availability exclusion filtering (ledger-first) for accommodation search {SearchId}",
                    searchId
                );

                // Build a quick candidate set using cheap DB filters (active rooms + price exclusion + guest capacity)
                var candidateQuery = _context.Rooms.Where(r => r.IsActive);
                if (excludedRoomIds != null && excludedRoomIds.Any())
                    candidateQuery = candidateQuery.Where(r => !excludedRoomIds.Contains(r.Id));
                if (searchRequest.Guests > 0)
                    candidateQuery = candidateQuery.Where(r => r.MaxGuests >= searchRequest.Guests);
                if (searchRequest.SpecificAccommodationIds?.Any() == true)
                    candidateQuery = candidateQuery.Where(r =>
                        r.AccommodationId.HasValue
                        && searchRequest.SpecificAccommodationIds.Contains(r.AccommodationId.Value)
                    );

                // Limit to a reasonable cap to avoid huge HMGET storms
                var candidateIds = await candidateQuery.Select(r => r.Id).Take(5000).ToListAsync();

                var unavailableFromLedger = new List<int>();
                var unavailableFromCalendar = new List<int>();

                if (candidateIds.Any())
                {
                    Dictionary<int, int> ledgerMap = new Dictionary<int, int>();
                    try
                    {
                        if (
                            searchRequest.CheckInDate.HasValue
                            && searchRequest.CheckOutDate.HasValue
                        )
                        {
                            ledgerMap = await _ledgerService.TryGetMinAvailableFromLedgerAsync(
                                candidateIds,
                                searchRequest.CheckInDate.Value,
                                searchRequest.CheckOutDate.Value
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Availability ledger read failed for accommodation search {SearchId}, falling back to calendar for availability",
                            searchId
                        );
                        ledgerMap = new Dictionary<int, int>();
                    }

                    // Rooms that ledger says have minAvailable < requested quantity are unavailable
                    var requiredUnits = Math.Max(1, searchRequest.Quantity);
                    unavailableFromLedger = ledgerMap
                        .Where(kv => kv.Value < requiredUnits)
                        .Select(kv => kv.Key)
                        .ToList();

                    // Rooms missing from ledger need calendar-based check for min available units
                    var missingLedgerIds = candidateIds.Except(ledgerMap.Keys).ToList();
                    if (
                        missingLedgerIds.Any()
                        && searchRequest.CheckInDate.HasValue
                        && searchRequest.CheckOutDate.HasValue
                    )
                    {
                        var fallback = await _calendarService.GetMinAvailableUnitsForRoomsAsync(
                            missingLedgerIds,
                            searchRequest.CheckInDate.Value,
                            searchRequest.CheckOutDate.Value
                        );
                        unavailableFromCalendar = fallback
                            .Where(kv => kv.Value < requiredUnits)
                            .Select(kv => kv.Key)
                            .ToList();
                    }
                }

                // Merge ledger and calendar unavailable ids into a List for EF translation
                var unavailableRoomIds = unavailableFromLedger
                    .Concat(unavailableFromCalendar)
                    .Distinct()
                    .ToList();

                _logger.LogDebug(
                    "Availability exclusion (ledger-first) for accommodation search {SearchId}: {UnavailableCount} unavailable rooms to exclude",
                    searchId,
                    unavailableRoomIds.Count
                );

                // PHASE 3: Build accommodation query with room exclusions
                var accommodationQuery = BuildAccommodationQueryWithExclusions(
                    searchRequest,
                    excludedRoomIds,
                    unavailableRoomIds
                );

                // Get total count for pagination
                var totalAccommodations = await accommodationQuery.CountAsync();

                if (totalAccommodations == 0)
                {
                    return CreateEmptySearchResponse(searchRequest, searchId, stopwatch.Elapsed);
                }

                // PHASE 4: Apply sorting and pagination
                var sortedQuery = ApplyAccommodationSortingAndPagination(
                    accommodationQuery,
                    searchRequest
                );

                // PHASE 5: Execute the final query with optimized includes
                var accommodations = await sortedQuery
                    .Include(a =>
                        a.Rooms.Where(r =>
                            r.IsActive
                            && (excludedRoomIds == null || !excludedRoomIds.Contains(r.Id))
                            && !unavailableRoomIds.Contains(r.Id)
                            && (searchRequest.Guests <= 1 || r.MaxGuests >= searchRequest.Guests)
                        )
                    )
                    .ThenInclude(r =>
                        r.Photos.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).Take(3)
                    )
                    .Include(a =>
                        a.Rooms.Where(r =>
                            r.IsActive
                            && (excludedRoomIds == null || !excludedRoomIds.Contains(r.Id))
                            && !unavailableRoomIds.Contains(r.Id)
                            && (searchRequest.Guests <= 1 || r.MaxGuests >= searchRequest.Guests)
                        )
                    )
                    .ThenInclude(r => r.RoomAmenities.Take(5))
                    .ThenInclude(ra => ra.Amenity)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogDebug(
                    "Database query for accommodation search {SearchId}: retrieved {Count} accommodations for final processing",
                    searchId,
                    accommodations.Count
                );

                // PHASE 6: Calculate exact pricing only for final results
                var allAvailableRooms = accommodations
                    .SelectMany(a => a.Rooms)
                    .Where(r =>
                        r.IsActive
                        && (excludedRoomIds == null || !excludedRoomIds.Contains(r.Id))
                        && !unavailableRoomIds.Contains(r.Id)
                    )
                    .ToList();

                var roomIds = allAvailableRooms.Select(r => r.Id).ToList();
                var roomPrices = await _calendarService.GetRoomPricesAsync(
                    roomIds,
                    searchRequest.CheckInDate ?? DateTime.Today,
                    searchRequest.CheckOutDate ?? DateTime.Today.AddDays(1)
                );

                // PHASE 7: Build accommodation search results with exact pricing
                var results = await BuildAccommodationSearchResultsOptimized(
                    accommodations,
                    roomPrices,
                    searchRequest,
                    excludedRoomIds,
                    unavailableRoomIds
                );

                // Convert roomPrices (which are total-stay amounts) to per-night averages for metadata/stats
                var nights =
                    searchRequest.CheckInDate.HasValue && searchRequest.CheckOutDate.HasValue
                        ? (searchRequest.CheckOutDate.Value - searchRequest.CheckInDate.Value).Days
                        : 1;
                var nightlyPriceValues = roomPrices
                    .Values.Select(v => nights > 0 ? v / nights : v)
                    .ToList();

                var stats = BuildSearchStatsOptimized(
                    accommodations,
                    allAvailableRooms,
                    nightlyPriceValues
                );

                var response = new AccommodationSearchResponseDTO
                {
                    Results = results,
                    Metadata = BuildSearchMetadata(
                        totalAccommodations,
                        searchRequest,
                        searchId,
                        stopwatch.Elapsed,
                        nightlyPriceValues
                    ),
                    AppliedFilters = BuildAppliedFilters(searchRequest),
                    Stats = stats,
                };

                // Cache the results
                await CacheAccommodationSearchResultAsync(cacheKey, response);

                _logger.LogInformation(
                    "Completed optimized accommodation search {SearchId} in {Duration}ms: {Results} accommodations from {Total} total matches",
                    searchId,
                    stopwatch.ElapsedMilliseconds,
                    results.Count,
                    totalAccommodations
                );

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in optimized accommodation search {SearchId}",
                    searchId
                );
                throw;
            }
        }

        private IQueryable<Accommodation> BuildAccommodationQueryWithExclusions(
            AccommodationSearchRequestDTO searchRequest,
            List<int>? excludedRoomIds,
            List<int> unavailableRoomIds
        )
        {
            // Only include accommodations that are approved. Relying on Status ensures Pending/Rejected/Suspended
            // accommodations are excluded from public searches even if IsActive is inconsistent.
            var query = _context.Accommodations.Where(a =>
                a.Status == Models.Enums.AccommodationStatus.Approved
            );

            // Determine if user has applied ANY filters
            bool hasFilters =
                searchRequest.CheckInDate.HasValue
                || searchRequest.CheckOutDate.HasValue
                || searchRequest.MinPrice.HasValue
                || searchRequest.MaxPrice.HasValue
                || searchRequest.RequiredAmenityIds.Any()
                || searchRequest.MinRoomsAvailable.HasValue
                || searchRequest.Guests > 1;

            // Only filter by rooms if user has applied ANY filters
            // Otherwise, show ALL approved accommodations (even those without rooms)
            if (hasFilters)
            {
                // Only include accommodations that have at least one available room that meets criteria
                query = query.Where(a =>
                    a.Rooms.Any(r =>
                        r.IsActive
                        &&
                        // Exclude by price (if price filter applied)
                        (excludedRoomIds == null || !excludedRoomIds.Contains(r.Id))
                        &&
                        // Exclude unavailable rooms
                        !unavailableRoomIds.Contains(r.Id)
                        &&
                        // Guest capacity filter
                        (searchRequest.Guests <= 1 || r.MaxGuests >= searchRequest.Guests)
                        &&
                        // Amenity filters (if specified)
                        (
                            !searchRequest.RequiredAmenityIds.Any()
                            || r.RoomAmenities.Any(ra =>
                                searchRequest.RequiredAmenityIds.Contains(ra.AmenityId)
                            )
                        )
                    )
                );
            }

            // Apply accommodation-level filters
            if (!string.IsNullOrWhiteSpace(searchRequest.SearchTerm))
            {
                var searchTerm = searchRequest.SearchTerm.ToLower();
                query = query.Where(a =>
                    a.Name.ToLower().Contains(searchTerm)
                    || (a.Description != null && a.Description.ToLower().Contains(searchTerm))
                );
            }

            if (searchRequest.SpecificAccommodationIds?.Any() == true)
            {
                query = query.Where(a => searchRequest.SpecificAccommodationIds.Contains(a.Id));
            }

            // Minimum available rooms filter
            if (searchRequest.MinRoomsAvailable.HasValue)
            {
                query = query.Where(a =>
                    a.Rooms.Count(r =>
                        r.IsActive
                        && (excludedRoomIds == null || !excludedRoomIds.Contains(r.Id))
                        && !unavailableRoomIds.Contains(r.Id)
                        && (searchRequest.Guests <= 1 || r.MaxGuests >= searchRequest.Guests)
                    ) >= searchRequest.MinRoomsAvailable.Value
                );
            }

            return query;
        }

        private IQueryable<Accommodation> ApplyAccommodationSortingAndPagination(
            IQueryable<Accommodation> query,
            AccommodationSearchRequestDTO searchRequest
        )
        {
            // Apply sorting
            var sortedQuery = searchRequest.SortBy switch
            {
                AccommodationSearchSortBy.Name => searchRequest.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(a => a.Name)
                    : query.OrderByDescending(a => a.Name),

                AccommodationSearchSortBy.LastUpdated => searchRequest.SortOrder
                == SortOrder.Ascending
                    ? query.OrderBy(a => a.UpdatedAt)
                    : query.OrderByDescending(a => a.UpdatedAt),

                AccommodationSearchSortBy.AvailableRooms => searchRequest.SortOrder
                == SortOrder.Ascending
                    ? query.OrderBy(a => a.Rooms.Count(r => r.IsActive))
                    : query.OrderByDescending(a => a.Rooms.Count(r => r.IsActive)),

                AccommodationSearchSortBy.Price => searchRequest.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(a => a.Rooms.Where(r => r.IsActive).Min(r => r.DefaultPrice))
                    : query.OrderByDescending(a =>
                        a.Rooms.Where(r => r.IsActive).Min(r => r.DefaultPrice)
                    ),

                _ => query.OrderBy(a => a.Name), // Default sorting
            };

            // Apply pagination
            return sortedQuery
                .Skip((searchRequest.Page - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize);
        }

        private async Task<
            List<AccommodationSearchResultDTO>
        > BuildAccommodationSearchResultsOptimized(
            List<Accommodation> accommodations,
            Dictionary<int, decimal> roomPrices,
            AccommodationSearchRequestDTO searchRequest,
            List<int>? excludedRoomIds,
            List<int> unavailableRoomIds
        )
        {
            var results = new List<AccommodationSearchResultDTO>();
            var nights =
                searchRequest.CheckInDate.HasValue && searchRequest.CheckOutDate.HasValue
                    ? (searchRequest.CheckOutDate.Value - searchRequest.CheckInDate.Value).Days
                    : 1;
            foreach (var accommodation in accommodations)
            {
                // Filter to only available rooms that meet criteria
                var availableRooms = accommodation
                    .Rooms.Where(r =>
                        r.IsActive
                        && (excludedRoomIds == null || !excludedRoomIds.Contains(r.Id))
                        && !unavailableRoomIds.Contains(r.Id)
                        && (searchRequest.Guests <= 1 || r.MaxGuests >= searchRequest.Guests)
                    )
                    .ToList();

                if (!availableRooms.Any() && !searchRequest.IncludeUnavailableAccommodations)
                    continue;

                // Build available rooms DTOs with optimized pricing
                var roomDtos = new List<AvailableRoomDTO>();
                foreach (var room in availableRooms)
                {
                    // roomPrices contains the total price for the stay (sum of per-night prices)
                    var totalPrice = roomPrices.GetValueOrDefault(
                        room.Id,
                        room.DefaultPrice * nights
                    );
                    var calculatedPrice = nights > 0 ? totalPrice / nights : totalPrice;

                    roomDtos.Add(
                        new AvailableRoomDTO
                        {
                            Id = room.Id,
                            Name = room.Name,
                            Description = room.Description,
                            DefaultPrice = room.DefaultPrice,
                            MaxGuests = room.MaxGuests,
                            CalculatedPrice = calculatedPrice,
                            TotalPrice = totalPrice,
                            MainPhotoUrl =
                                room.Photos.FirstOrDefault(p => p.IsActive)?.CdnUrl
                                ?? room.Photos.FirstOrDefault(p => p.IsActive)?.S3Url,
                            PhotoUrls = room
                                .Photos.Where(p => p.IsActive)
                                .Take(3)
                                .Select(p => p.CdnUrl ?? p.S3Url)
                                .ToList(),
                            TopAmenities = room
                                .RoomAmenities.Take(5)
                                .Select(ra => new AmenityDTO
                                {
                                    Id = ra.Amenity.Id,
                                    Name = ra.Amenity.Name,
                                    Category = ra.Amenity.Category,
                                    Description = ra.Amenity.Description,
                                    IsActive = ra.Amenity.IsActive,
                                    LastModified = ra.Amenity.LastModified,
                                    DisplayOrder = ra.Amenity.DisplayOrder,
                                })
                                .ToList(),
                            TotalAmenities = room.RoomAmenities.Count,
                            IsAvailable = true,
                            AvailabilityStatus = "Available",
                            RoomRelevanceScore = CalculateRoomRelevanceScore(room, searchRequest),
                            LastUpdated = room.UpdatedAt,
                            DailyPrices = new List<DailyPriceDTO>(), // Can be populated if needed
                        }
                    );
                }

                // Calculate accommodation-level statistics
                var calculatedPrices = roomDtos
                    .Where(r => r.CalculatedPrice.HasValue)
                    .Select(r => r.CalculatedPrice!.Value)
                    .ToList();
                var totalPrices = roomDtos
                    .Where(r => r.TotalPrice.HasValue)
                    .Select(r => r.TotalPrice!.Value)
                    .ToList();

                // Get all unique amenities across all rooms
                var allAmenities = availableRooms
                    .SelectMany(r => r.RoomAmenities.Select(ra => ra.Amenity.Name))
                    .GroupBy(name => name)
                    .OrderByDescending(g => g.Count())
                    .Take(8)
                    .Select(g => g.Key)
                    .ToList();

                results.Add(
                    new AccommodationSearchResultDTO
                    {
                        Id = accommodation.Id,
                        Name = accommodation.Name,
                        Description = accommodation.Description,
                        Logo = accommodation.Logo,
                        Address = accommodation.Address,
                        EmailAddress = accommodation.EmailAddress,
                        ContactNo = accommodation.ContactNo,
                        IsActive = accommodation.IsActive,
                        AvailableRooms = roomDtos,
                        TotalAvailableRooms = roomDtos.Count,
                        TotalRoomsInAccommodation = accommodation.Rooms.Count(r => r.IsActive),
                        LowestRoomPrice = calculatedPrices.Any() ? calculatedPrices.Min() : null,
                        HighestRoomPrice = calculatedPrices.Any() ? calculatedPrices.Max() : null,
                        AverageRoomPrice = calculatedPrices.Any()
                            ? calculatedPrices.Average()
                            : null,
                        TotalStayPrice = totalPrices.Any() ? totalPrices.Min() : null, // Cheapest room's total
                        PopularAmenities = allAmenities,
                        TotalUniqueAmenities = allAmenities.Count,
                        RelevanceScore = CalculateRelevanceScore(accommodation, searchRequest),
                        PopularityScore = accommodation.Rooms.Count(r => r.IsActive), // Simple popularity metric
                        LastUpdated = accommodation.UpdatedAt,
                        MainImageUrl = accommodation.Logo, // Could be expanded to include accommodation images
                        ImageUrls = new List<string>(),
                    }
                );
            }

            return results;
        }

        private async Task<IQueryable<Accommodation>> ApplyRoomFilters(
            IQueryable<Accommodation> query,
            AccommodationSearchRequestDTO searchRequest,
            List<int> availableRoomIds
        )
        {
            // Filter accommodations that have rooms meeting the criteria
            return query.Where(a =>
                a.Rooms.Any(r =>
                    availableRoomIds.Contains(r.Id)
                    && r.IsActive
                    && (searchRequest.Guests <= 1 || r.MaxGuests >= searchRequest.Guests)
                    && (
                        !searchRequest.MinPrice.HasValue
                        || r.DefaultPrice >= searchRequest.MinPrice.Value
                    )
                    && (
                        !searchRequest.MaxPrice.HasValue
                        || r.DefaultPrice <= searchRequest.MaxPrice.Value
                    )
                    && (
                        !searchRequest.RequiredAmenityIds.Any()
                        || r.RoomAmenities.Any(ra =>
                            searchRequest.RequiredAmenityIds.Contains(ra.AmenityId)
                        )
                    )
                )
            );
        }

        private IQueryable<Accommodation> ApplySorting(
            IQueryable<Accommodation> query,
            AccommodationSearchRequestDTO searchRequest
        )
        {
            return searchRequest.SortBy switch
            {
                AccommodationSearchSortBy.Name => searchRequest.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(a => a.Name)
                    : query.OrderByDescending(a => a.Name),

                AccommodationSearchSortBy.LastUpdated => searchRequest.SortOrder
                == SortOrder.Ascending
                    ? query.OrderBy(a => a.UpdatedAt)
                    : query.OrderByDescending(a => a.UpdatedAt),

                AccommodationSearchSortBy.AvailableRooms => searchRequest.SortOrder
                == SortOrder.Ascending
                    ? query.OrderBy(a => a.Rooms.Count(r => r.IsActive))
                    : query.OrderByDescending(a => a.Rooms.Count(r => r.IsActive)),

                AccommodationSearchSortBy.Price => searchRequest.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(a => a.Rooms.Where(r => r.IsActive).Min(r => r.DefaultPrice))
                    : query.OrderByDescending(a =>
                        a.Rooms.Where(r => r.IsActive).Min(r => r.DefaultPrice)
                    ),

                _ => query.OrderBy(a => a.Name), // Default sorting
            };
        }

        private async Task<List<AccommodationSearchResultDTO>> BuildAccommodationSearchResults(
            List<Accommodation> accommodations,
            AccommodationSearchRequestDTO searchRequest,
            List<int> availableRoomIds
        )
        {
            var results = new List<AccommodationSearchResultDTO>();
            var nights =
                searchRequest.CheckInDate.HasValue && searchRequest.CheckOutDate.HasValue
                    ? (searchRequest.CheckOutDate.Value - searchRequest.CheckInDate.Value).Days
                    : 1;

            foreach (var accommodation in accommodations)
            {
                // Filter rooms that are available and meet criteria
                var availableRooms = accommodation
                    .Rooms.Where(r =>
                        availableRoomIds.Contains(r.Id)
                        && r.IsActive
                        && (searchRequest.Guests <= 1 || r.MaxGuests >= searchRequest.Guests)
                    )
                    .ToList();

                if (!availableRooms.Any() && !searchRequest.IncludeUnavailableAccommodations)
                    continue;

                // Build available rooms DTOs
                var roomDtos = new List<AvailableRoomDTO>();
                foreach (var room in availableRooms)
                {
                    var calculatedPrice = searchRequest.CheckInDate.HasValue
                        ? await _calendarService.GetPriceForDateAsync(
                            room.Id,
                            searchRequest.CheckInDate.Value
                        )
                        : room.DefaultPrice;

                    var totalPrice =
                        searchRequest.CheckInDate.HasValue && searchRequest.CheckOutDate.HasValue
                            ? await _calendarService.CalculateStayTotalAsync(
                                room.Id,
                                searchRequest.CheckInDate.Value,
                                searchRequest.CheckOutDate.Value
                            )
                            : room.DefaultPrice;

                    roomDtos.Add(
                        new AvailableRoomDTO
                        {
                            Id = room.Id,
                            Name = room.Name,
                            Description = room.Description,
                            DefaultPrice = room.DefaultPrice,
                            MaxGuests = room.MaxGuests,
                            CalculatedPrice = calculatedPrice,
                            TotalPrice = totalPrice,
                            MainPhotoUrl = room.MainPhotoUrl,
                            PhotoUrls = room
                                .Photos.Where(p => p.IsActive)
                                .Take(3)
                                .Select(p => p.CdnUrl ?? p.S3Url)
                                .ToList(),
                            TopAmenities = room
                                .RoomAmenities.Take(5)
                                .Select(ra => new AmenityDTO
                                {
                                    Id = ra.Amenity.Id,
                                    Name = ra.Amenity.Name,
                                    Category = ra.Amenity.Category,
                                })
                                .ToList(),
                            TotalAmenities = room.RoomAmenities.Count,
                            IsAvailable = true,
                            AvailabilityStatus = "Available",
                            RoomRelevanceScore = 1.0f, // Calculate based on criteria match
                            LastUpdated = room.UpdatedAt,
                        }
                    );
                }

                // Calculate accommodation-level statistics
                var prices = roomDtos
                    .Where(r => r.CalculatedPrice.HasValue)
                    .Select(r => r.CalculatedPrice!.Value)
                    .ToList();
                var totalPrices = roomDtos
                    .Where(r => r.TotalPrice.HasValue)
                    .Select(r => r.TotalPrice!.Value)
                    .ToList();

                // Get all unique amenities across all rooms
                var allAmenities = availableRooms
                    .SelectMany(r => r.RoomAmenities.Select(ra => ra.Amenity.Name))
                    .GroupBy(name => name)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => g.Key)
                    .ToList();

                results.Add(
                    new AccommodationSearchResultDTO
                    {
                        Id = accommodation.Id,
                        Name = accommodation.Name,
                        Description = accommodation.Description,
                        Logo = accommodation.Logo,
                        IsActive = accommodation.IsActive,
                        AvailableRooms = roomDtos,
                        TotalAvailableRooms = roomDtos.Count,
                        TotalRoomsInAccommodation = accommodation.Rooms.Count(r => r.IsActive),
                        LowestRoomPrice = prices.Any() ? prices.Min() : null,
                        HighestRoomPrice = prices.Any() ? prices.Max() : null,
                        AverageRoomPrice = prices.Any() ? prices.Average() : null,
                        TotalStayPrice = totalPrices.Any() ? totalPrices.Min() : null, // Cheapest room's total
                        PopularAmenities = allAmenities,
                        TotalUniqueAmenities = allAmenities.Count,
                        RelevanceScore = CalculateRelevanceScore(accommodation, searchRequest),
                        PopularityScore = accommodation.Rooms.Count(r => r.IsActive), // Simple popularity metric
                        LastUpdated = accommodation.UpdatedAt,
                        MainImageUrl = accommodation.Logo, // Could be expanded to include accommodation images
                        ImageUrls = new List<string>(),
                    }
                );
            }

            return results;
        }

        private float CalculateRelevanceScore(
            Accommodation accommodation,
            AccommodationSearchRequestDTO searchRequest
        )
        {
            float score = 1.0f;

            // Boost score based on search term match
            if (!string.IsNullOrWhiteSpace(searchRequest.SearchTerm))
            {
                var searchTerm = searchRequest.SearchTerm.ToLower();
                if (accommodation.Name.ToLower().Contains(searchTerm))
                    score += 2.0f;
                if (accommodation.Description?.ToLower().Contains(searchTerm) == true)
                    score += 1.0f;
            }

            // Boost score based on number of available rooms
            var activeRooms = accommodation.Rooms.Count(r => r.IsActive);
            score += activeRooms * 0.1f;

            return score;
        }

        // Helper methods for caching and utilities
        private string GenerateAccommodationSearchCacheKey(
            AccommodationSearchRequestDTO searchRequest
        )
        {
            var keyParts = new[]
            {
                $"accom_search",
                searchRequest.CheckInDate?.ToString("yyyyMMdd") ?? "null",
                searchRequest.CheckOutDate?.ToString("yyyyMMdd") ?? "null",
                searchRequest.Guests.ToString(),
                searchRequest.Page.ToString(),
                searchRequest.PageSize.ToString(),
                searchRequest.MinPrice?.ToString() ?? "null",
                searchRequest.MaxPrice?.ToString() ?? "null",
                string.Join(",", searchRequest.RequiredAmenityIds.OrderBy(x => x)),
                searchRequest.SearchTerm ?? "null",
                searchRequest.SortBy.ToString(),
                searchRequest.SortOrder.ToString(),
                searchRequest.MinRoomsAvailable?.ToString() ?? "null",
            };

            var key = string.Join("_", keyParts);
            return $"accommodation_search_{key.GetHashCode():X8}";
        }

        private async Task<AccommodationSearchResponseDTO?> GetCachedAccommodationSearchResultAsync(
            string cacheKey
        )
        {
            return await _cacheService.GetAsync<AccommodationSearchResponseDTO>(cacheKey);
        }

        private async Task CacheAccommodationSearchResultAsync(
            string cacheKey,
            AccommodationSearchResponseDTO response
        )
        {
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15)); // Cache for 15 minutes
        }

        private float CalculateRoomRelevanceScore(
            Room room,
            AccommodationSearchRequestDTO searchRequest
        )
        {
            float score = 1.0f;

            // Boost score if room meets guest requirements perfectly
            if (room.MaxGuests == searchRequest.Guests)
                score += 0.5f;
            else if (room.MaxGuests >= searchRequest.Guests)
                score += 0.2f;

            // Boost score based on amenities match
            if (searchRequest.RequiredAmenityIds.Any())
            {
                var matchingAmenities = room.RoomAmenities.Count(ra =>
                    searchRequest.RequiredAmenityIds.Contains(ra.AmenityId)
                );
                score += (matchingAmenities * 0.1f);
            }

            return score;
        }

        private SearchMetadataDTO BuildSearchMetadata(
            int totalResults,
            AccommodationSearchRequestDTO searchRequest,
            string searchId,
            TimeSpan duration,
            IEnumerable<decimal> prices
        )
        {
            var priceList = prices.ToList();

            return new SearchMetadataDTO
            {
                TotalResults = totalResults,
                CurrentPage = searchRequest.Page,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalResults / searchRequest.PageSize),
                HasNextPage = searchRequest.Page * searchRequest.PageSize < totalResults,
                HasPreviousPage = searchRequest.Page > 1,
                SearchDuration = duration,
                SearchId = searchId,
                CacheHit = false,
                MinPrice = priceList.Any() ? priceList.Min() : null,
                MaxPrice = priceList.Any() ? priceList.Max() : null,
                AveragePrice = priceList.Any() ? priceList.Average() : null,
                AvailableRooms = priceList.Count(),
                UnavailableRooms = 0, // Will be calculated properly if needed
                IsSmartPagination = false,
                ActualResultsFound = totalResults,
            };
        }

        private AccommodationSearchStatsDTO BuildSearchStatsOptimized(
            List<Accommodation> accommodations,
            List<Room> allAvailableRooms,
            IEnumerable<decimal> nightlyPrices
        )
        {
            var prices = nightlyPrices.ToList();

            return new AccommodationSearchStatsDTO
            {
                TotalAccommodations = accommodations.Count,
                AccommodationsWithAvailability = accommodations.Count(a => a.Rooms.Any()),
                TotalAvailableRooms = allAvailableRooms.Count,
                TotalSearchedRooms = allAvailableRooms.Count,
                GlobalMinPrice = prices.Any() ? prices.Min() : null,
                GlobalMaxPrice = prices.Any() ? prices.Max() : null,
                GlobalAveragePrice = prices.Any() ? prices.Average() : null,
                AvailabilityDistribution = BuildAvailabilityDistributionOptimized(
                    accommodations,
                    allAvailableRooms
                ),
            };
        }

        private Dictionary<string, int> BuildAvailabilityDistributionOptimized(
            List<Accommodation> accommodations,
            List<Room> allAvailableRooms
        )
        {
            var distribution = new Dictionary<string, int>
            {
                ["1-2 rooms"] = 0,
                ["3-5 rooms"] = 0,
                ["6-10 rooms"] = 0,
                ["11+ rooms"] = 0,
            };

            foreach (var accommodation in accommodations)
            {
                var availableCount = allAvailableRooms.Count(r =>
                    r.AccommodationId == accommodation.Id
                );
                if (availableCount <= 2)
                    distribution["1-2 rooms"]++;
                else if (availableCount <= 5)
                    distribution["3-5 rooms"]++;
                else if (availableCount <= 10)
                    distribution["6-10 rooms"]++;
                else
                    distribution["11+ rooms"]++;
            }

            return distribution;
        }

        private SearchMetadataDTO BuildSearchMetadata(
            int totalResults,
            AccommodationSearchRequestDTO searchRequest,
            string searchId,
            TimeSpan duration
        )
        {
            return new SearchMetadataDTO
            {
                TotalResults = totalResults,
                CurrentPage = searchRequest.Page,
                PageSize = searchRequest.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalResults / searchRequest.PageSize),
                HasNextPage = searchRequest.Page * searchRequest.PageSize < totalResults,
                HasPreviousPage = searchRequest.Page > 1,
                SearchDuration = duration,
                SearchId = searchId,
                CacheHit = false,
            };
        }

        private SearchFiltersDTO BuildAppliedFilters(AccommodationSearchRequestDTO searchRequest)
        {
            return new SearchFiltersDTO
            {
                CheckInDate = searchRequest.CheckInDate ?? DateTime.Today,
                CheckOutDate = searchRequest.CheckOutDate ?? DateTime.Today.AddDays(1),
                Guests = searchRequest.Guests,
                MinPrice = searchRequest.MinPrice,
                MaxPrice = searchRequest.MaxPrice,
                SearchTerm = searchRequest.SearchTerm,
                SortBy = (RoomSearchSortBy)searchRequest.SortBy, // Cast or map appropriately
                SortOrder = searchRequest.SortOrder,
            };
        }

        // Remove unused helper methods and fix method signatures
        private AccommodationSearchStatsDTO BuildSearchStats(
            List<Accommodation> accommodations,
            List<int> availableRoomIds,
            AccommodationSearchRequestDTO searchRequest
        )
        {
            var availableRooms = accommodations
                .SelectMany(a => a.Rooms.Where(r => availableRoomIds.Contains(r.Id)))
                .ToList();
            var prices = availableRooms.Select(r => r.DefaultPrice).ToList();

            return new AccommodationSearchStatsDTO
            {
                TotalAccommodations = accommodations.Count,
                AccommodationsWithAvailability = accommodations.Count(a =>
                    a.Rooms.Any(r => availableRoomIds.Contains(r.Id))
                ),
                TotalAvailableRooms = availableRooms.Count,
                GlobalMinPrice = prices.Any() ? prices.Min() : null,
                GlobalMaxPrice = prices.Any() ? prices.Max() : null,
                GlobalAveragePrice = prices.Any() ? prices.Average() : null,
                AvailabilityDistribution = BuildAvailabilityDistribution(
                    accommodations,
                    availableRoomIds
                ),
            };
        }

        private Dictionary<string, int> BuildAvailabilityDistribution(
            List<Accommodation> accommodations,
            List<int> availableRoomIds
        )
        {
            var distribution = new Dictionary<string, int>
            {
                ["1-2 rooms"] = 0,
                ["3-5 rooms"] = 0,
                ["6-10 rooms"] = 0,
                ["11+ rooms"] = 0,
            };

            foreach (var accommodation in accommodations)
            {
                var availableCount = accommodation.Rooms.Count(r =>
                    availableRoomIds.Contains(r.Id)
                );
                if (availableCount <= 2)
                    distribution["1-2 rooms"]++;
                else if (availableCount <= 5)
                    distribution["3-5 rooms"]++;
                else if (availableCount <= 10)
                    distribution["6-10 rooms"]++;
                else
                    distribution["11+ rooms"]++;
            }

            return distribution;
        }

        private AccommodationSearchResponseDTO CreateEmptySearchResponse(
            AccommodationSearchRequestDTO searchRequest,
            string searchId,
            TimeSpan duration
        )
        {
            return new AccommodationSearchResponseDTO
            {
                Results = new List<AccommodationSearchResultDTO>(),
                Metadata = BuildSearchMetadata(
                    0,
                    searchRequest,
                    searchId,
                    duration,
                    new List<decimal>()
                ),
                AppliedFilters = BuildAppliedFilters(searchRequest),
                Stats = new AccommodationSearchStatsDTO(),
            };
        }

        private void ValidateSearchRequest(AccommodationSearchRequestDTO searchRequest)
        {
            if (searchRequest.CheckInDate.HasValue && searchRequest.CheckOutDate.HasValue)
            {
                if (searchRequest.CheckOutDate <= searchRequest.CheckInDate)
                    throw new ArgumentException("Check-out date must be after check-in date");
            }
            if (searchRequest.Guests < 1)
                throw new ArgumentException("Number of guests must be at least 1");

            if (searchRequest.Page < 1)
                throw new ArgumentException("Page number must be at least 1");

            if (searchRequest.PageSize < 1 || searchRequest.PageSize > 50)
                throw new ArgumentException("Page size must be between 1 and 50");
        }

        // Placeholder implementations for interface methods
        public async Task<List<AccommodationQuickSearchResultDTO>> QuickSearchAccommodationsAsync(
            string searchTerm,
            int limit = 10
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                {
                    return new List<AccommodationQuickSearchResultDTO>();
                }

                var normalizedTerm = searchTerm.ToLower().Trim();

                var results = await _context
                    .Accommodations.Where(a =>
                        a.Status == Models.Enums.AccommodationStatus.Approved
                        && (
                            a.Name.ToLower().Contains(normalizedTerm)
                            || (
                                a.Description != null
                                && a.Description.ToLower().Contains(normalizedTerm)
                            )
                        )
                    )
                    .Select(a => new AccommodationQuickSearchResultDTO
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        Logo = a.Logo,
                        AvailableRooms = a.Rooms.Count(r => r.IsActive),
                        LowestPrice = a
                            .Rooms.Where(r => r.IsActive)
                            .Min(r => (decimal?)r.DefaultPrice),
                        TopAmenities = a
                            .Rooms.Where(r => r.IsActive)
                            .SelectMany(r => r.RoomAmenities)
                            .GroupBy(ra => ra.Amenity.Name)
                            .OrderByDescending(g => g.Count())
                            .Take(3)
                            .Select(g => g.Key)
                            .ToList(),
                    })
                    .Take(limit)
                    .OrderBy(r => r.Name)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogDebug(
                    "Quick search for '{SearchTerm}' returned {Count} accommodations",
                    searchTerm,
                    results.Count
                );
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in quick accommodation search for term: {SearchTerm}",
                    searchTerm
                );
                return new List<AccommodationQuickSearchResultDTO>();
            }
        }

        public async Task<AccommodationSearchSuggestionResponseDTO> GetAccommodationSearchSuggestionsAsync(
            string query,
            int limit = 10
        )
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return new AccommodationSearchSuggestionResponseDTO
                {
                    AccommodationNames = new List<string>(),
                    PopularAmenities = new List<string>(),
                    PopularPriceRanges = new List<decimal>(),
                    Locations = new List<string>(),
                };
            }

            try
            {
                var searchTerm = query.ToLower().Trim();

                // Get matching accommodation names
                var accommodationNames = await _context
                    .Accommodations.Where(a =>
                        a.Status == Models.Enums.AccommodationStatus.Approved
                        && a.Name.ToLower().Contains(searchTerm)
                    )
                    .Select(a => a.Name)
                    .Distinct()
                    .Take(limit)
                    .OrderBy(name => name)
                    .ToListAsync();

                // Get popular amenities for matching accommodations
                var popularAmenities = await _context
                    .Accommodations.Where(a =>
                        a.Status == Models.Enums.AccommodationStatus.Approved
                        && a.Name.ToLower().Contains(searchTerm)
                    )
                    .SelectMany(a => a.Rooms)
                    .Where(r => r.IsActive)
                    .SelectMany(r => r.RoomAmenities)
                    .GroupBy(ra => ra.Amenity.Name)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => g.Key)
                    .ToListAsync();

                // Get popular price ranges (simplified)
                var popularPrices = await _context
                    .Accommodations.Where(a =>
                        a.Status == Models.Enums.AccommodationStatus.Approved
                        && a.Name.ToLower().Contains(searchTerm)
                    )
                    .SelectMany(a => a.Rooms)
                    .Where(r => r.IsActive)
                    .Select(r => r.DefaultPrice)
                    .OrderBy(price => price)
                    .Take(3)
                    .ToListAsync();

                return new AccommodationSearchSuggestionResponseDTO
                {
                    AccommodationNames = accommodationNames,
                    PopularAmenities = popularAmenities,
                    PopularPriceRanges = popularPrices,
                    Locations = new List<string>(), // Can be expanded if location data is available
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting accommodation search suggestions for query: {Query}",
                    query
                );
                return new AccommodationSearchSuggestionResponseDTO
                {
                    AccommodationNames = new List<string>(),
                    PopularAmenities = new List<string>(),
                    PopularPriceRanges = new List<decimal>(),
                    Locations = new List<string>(),
                };
            }
        }

        public async Task WarmUpAccommodationCacheAsync()
        {
            try
            {
                _logger.LogInformation("Starting accommodation cache warmup process");
                var stopwatch = Stopwatch.StartNew();

                // Get common search patterns for the next 30 days
                var today = DateTime.Today;
                var endDate = today.AddDays(30);

                // Common search scenarios to pre-cache
                var commonSearches = new List<AccommodationSearchRequestDTO>
                {
                    new AccommodationSearchRequestDTO
                    {
                        CheckInDate = today.AddDays(1),
                        CheckOutDate = today.AddDays(2),
                        Guests = 2,
                        Page = 1,
                        PageSize = 10,
                    },
                    new AccommodationSearchRequestDTO
                    {
                        CheckInDate = today.AddDays(7),
                        CheckOutDate = today.AddDays(9),
                        Guests = 1,
                        Page = 1,
                        PageSize = 20,
                    },
                    new AccommodationSearchRequestDTO
                    {
                        CheckInDate = today.AddDays(14),
                        CheckOutDate = today.AddDays(16),
                        Guests = 4,
                        Page = 1,
                        PageSize = 15,
                    },
                };

                // Execute searches to populate cache
                var cacheWarmupTasks = commonSearches.Select(async searchRequest =>
                {
                    try
                    {
                        await SearchAccommodationsAsync(searchRequest);
                        _logger.LogDebug(
                            "Cache warmed for search: {CheckIn} to {CheckOut}, {Guests} guests",
                            searchRequest.CheckInDate,
                            searchRequest.CheckOutDate,
                            searchRequest.Guests
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Failed to warm cache for search: {CheckIn} to {CheckOut}",
                            searchRequest.CheckInDate,
                            searchRequest.CheckOutDate
                        );
                    }
                });

                await Task.WhenAll(cacheWarmupTasks);

                stopwatch.Stop();
                _logger.LogInformation(
                    "Accommodation cache warmup completed in {Duration}ms",
                    stopwatch.ElapsedMilliseconds
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during accommodation cache warmup");
                throw;
            }
        }
    }
}
