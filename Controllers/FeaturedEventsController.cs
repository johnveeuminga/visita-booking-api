using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitaBookingApi.Data;
using VisitaBookingApi.Models;

namespace VisitaBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeaturedEventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FeaturedEventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/FeaturedEvents - Get all active featured events (public)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeaturedEvent>>> GetActiveFeaturedEvents()
        {
            var events = await _context
                .FeaturedEvents.Where(e => e.IsActive)
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/FeaturedEvents/all - Get all featured events including inactive (admin only)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<FeaturedEvent>>> GetAllFeaturedEvents()
        {
            var events = await _context.FeaturedEvents.OrderBy(e => e.DisplayOrder).ToListAsync();

            return Ok(events);
        }

        // GET: api/FeaturedEvents/5 - Get specific featured event
        [HttpGet("{id}")]
        public async Task<ActionResult<FeaturedEvent>> GetFeaturedEvent(int id)
        {
            var featuredEvent = await _context.FeaturedEvents.FindAsync(id);

            if (featuredEvent == null)
            {
                return NotFound();
            }

            return Ok(featuredEvent);
        }

        // POST: api/FeaturedEvents - Create new featured event (admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FeaturedEvent>> CreateFeaturedEvent(FeaturedEventDto dto)
        {
            var featuredEvent = new FeaturedEvent
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ImageUrl = dto.ImageUrl,
                ImageUrl2 = dto.ImageUrl2,
                ImageUrl3 = dto.ImageUrl3,
                ImageUrl4 = dto.ImageUrl4,
                ImageUrl5 = dto.ImageUrl5,
                EventUrl = dto.EventUrl,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            _context.FeaturedEvents.Add(featuredEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetFeaturedEvent),
                new { id = featuredEvent.Id },
                featuredEvent
            );
        }

        // PUT: api/FeaturedEvents/5 - Update featured event (admin only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFeaturedEvent(int id, FeaturedEventDto dto)
        {
            var featuredEvent = await _context.FeaturedEvents.FindAsync(id);

            if (featuredEvent == null)
            {
                return NotFound();
            }

            featuredEvent.Title = dto.Title;
            featuredEvent.Description = dto.Description;
            featuredEvent.StartDate = dto.StartDate;
            featuredEvent.EndDate = dto.EndDate;
            featuredEvent.ImageUrl = dto.ImageUrl;
            featuredEvent.ImageUrl2 = dto.ImageUrl2;
            featuredEvent.ImageUrl3 = dto.ImageUrl3;
            featuredEvent.ImageUrl4 = dto.ImageUrl4;
            featuredEvent.ImageUrl5 = dto.ImageUrl5;
            featuredEvent.EventUrl = dto.EventUrl;
            featuredEvent.DisplayOrder = dto.DisplayOrder;
            featuredEvent.IsActive = dto.IsActive;
            featuredEvent.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/FeaturedEvents/5 - Delete featured event (admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFeaturedEvent(int id)
        {
            var featuredEvent = await _context.FeaturedEvents.FindAsync(id);

            if (featuredEvent == null)
            {
                return NotFound();
            }

            _context.FeaturedEvents.Remove(featuredEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/FeaturedEvents/5/toggle - Toggle active status (admin only)
        [HttpPut("{id}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleFeaturedEventStatus(int id)
        {
            var featuredEvent = await _context.FeaturedEvents.FindAsync(id);

            if (featuredEvent == null)
            {
                return NotFound();
            }

            featuredEvent.IsActive = !featuredEvent.IsActive;
            featuredEvent.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/FeaturedEvents/reorder - Update display order (admin only)
        [HttpPatch("reorder")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReorderFeaturedEvents(
            [FromBody] Dictionary<int, int> orderMap
        )
        {
            foreach (var kvp in orderMap)
            {
                var eventToUpdate = await _context.FeaturedEvents.FindAsync(kvp.Key);
                if (eventToUpdate != null)
                {
                    eventToUpdate.DisplayOrder = kvp.Value;
                    eventToUpdate.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
