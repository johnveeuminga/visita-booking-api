using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using visita_booking_api.Models.Enums;
using VisitaBookingApi.Data;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstablishmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EstablishmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Public: Search approved establishments
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? name,
            [FromQuery] string? category,
            [FromQuery] string? city,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            if (page < 1)
                page = 1;
            if (pageSize < 1 || pageSize > 100)
                pageSize = 20;

            var query = _context.Establishments.Where(e =>
                e.Status == EstablishmentStatus.Approved && e.IsActive
            );

            // Filter by name
            if (!string.IsNullOrEmpty(name))
                query = query.Where(e => e.Name.Contains(name));

            // Filter by category
            if (
                !string.IsNullOrEmpty(category)
                && Enum.TryParse<EstablishmentCategory>(category, true, out var categoryEnum)
            )
                query = query.Where(e => e.Category == categoryEnum);

            // Filter by city
            if (!string.IsNullOrEmpty(city))
                query = query.Where(e => e.City.Contains(city));

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.Logo,
                    e.CoverImage,
                    Category = e.Category.ToString(),
                    e.Address,
                    e.City,
                    e.ContactNumber,
                    e.Email,
                    e.Website,
                    e.FacebookPage,
                })
                .ToListAsync();

            return Ok(
                new
                {
                    success = true,
                    data = items,
                    pagination = new
                    {
                        total,
                        page,
                        pageSize,
                        totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    },
                }
            );
        }

        /// <summary>
        /// Public: Get establishment details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var establishment = await _context
                .Establishments.Include(e => e.Hours)
                .Where(e => e.Id == id && e.Status == EstablishmentStatus.Approved && e.IsActive)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.Logo,
                    e.CoverImage,
                    Category = e.Category.ToString(),
                    e.Address,
                    e.City,
                    e.Latitude,
                    e.Longitude,
                    e.ContactNumber,
                    e.Email,
                    e.Website,
                    e.FacebookPage,
                    e.CreatedAt,
                    Hours = e.Hours!.Select(h => new
                        {
                            DayOfWeek = h.DayOfWeek.ToString(),
                            OpenTime = h.OpenTime.HasValue
                                ? h.OpenTime.Value.ToString(@"hh\:mm")
                                : null,
                            CloseTime = h.CloseTime.HasValue
                                ? h.CloseTime.Value.ToString(@"hh\:mm")
                                : null,
                            h.IsClosed,
                        })
                        .OrderBy(h => h.DayOfWeek),
                })
                .FirstOrDefaultAsync();

            if (establishment == null)
                return NotFound(new { success = false, message = "Establishment not found" });

            return Ok(new { success = true, data = establishment });
        }

        /// <summary>
        /// Public: Get all categories
        /// </summary>
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            var categories = Enum.GetValues(typeof(EstablishmentCategory))
                .Cast<EstablishmentCategory>()
                .Select(c => new { value = c.ToString(), label = c.ToString() })
                .ToList();

            return Ok(new { success = true, data = categories });
        }

        /// <summary>
        /// Public: Get featured/popular establishments
        /// </summary>
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured([FromQuery] int limit = 6)
        {
            var establishments = await _context
                .Establishments.Where(e => e.Status == EstablishmentStatus.Approved && e.IsActive)
                .OrderByDescending(e => e.CreatedAt)
                .Take(limit)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.Logo,
                    Category = e.Category.ToString(),
                    e.City,
                })
                .ToListAsync();

            return Ok(new { success = true, data = establishments });
        }
    }
}
