using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/bulletinevents")]
    public class BulletinEventsController : ControllerBase
    {
        private readonly IBulletinEventService _bulletinEventService;
        private readonly ILogger<BulletinEventsController> _logger;

        public BulletinEventsController(
            IBulletinEventService bulletinEventService,
            ILogger<BulletinEventsController> logger
        )
        {
            _bulletinEventService = bulletinEventService;
            _logger = logger;
        }

        // GET: api/bulletinevents/calendar/{year}/{month}
        [HttpGet("calendar/{year}/{month}")]
        public async Task<ActionResult<BulletinCalendarMonth>> GetCalendarMonth(int year, int month)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest("Month must be between 1 and 12");
                }

                var calendar = await _bulletinEventService.GetCalendarMonthAsync(year, month);
                return Ok(calendar);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching bulletin calendar for {Year}-{Month}",
                    year,
                    month
                );
                return StatusCode(500, "An error occurred while fetching the calendar");
            }
        }

        // GET: api/bulletinevents
        [HttpGet]
        public async Task<ActionResult<List<BulletinEventResponse>>> GetAllEvents()
        {
            try
            {
                var events = await _bulletinEventService.GetAllEventsAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all bulletin events");
                return StatusCode(500, "An error occurred while fetching events");
            }
        }

        // GET: api/bulletinevents/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BulletinEventResponse>> GetEvent(int id)
        {
            try
            {
                var bulletinEvent = await _bulletinEventService.GetEventByIdAsync(id);
                return Ok(bulletinEvent);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Bulletin event with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bulletin event {Id}", id);
                return StatusCode(500, "An error occurred while fetching the event");
            }
        }

        // POST: api/bulletinevents
        [HttpPost]
        [Authorize(Roles = "Admin,EstablishmentOwner")]
        public async Task<ActionResult<BulletinEventResponse>> CreateEvent(
            [FromBody] CreateBulletinEventRequest request
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;

                var bulletinEvent = await _bulletinEventService.CreateEventAsync(request, userId);
                return CreatedAtAction(
                    nameof(GetEvent),
                    new { id = bulletinEvent.Id },
                    bulletinEvent
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulletin event");
                return StatusCode(500, "An error occurred while creating the event");
            }
        }

        // PUT: api/bulletinevents/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,EstablishmentOwner")]
        public async Task<ActionResult<BulletinEventResponse>> UpdateEvent(
            int id,
            [FromBody] UpdateBulletinEventRequest request
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var bulletinEvent = await _bulletinEventService.UpdateEventAsync(id, request);
                return Ok(bulletinEvent);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Bulletin event with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bulletin event {Id}", id);
                return StatusCode(500, "An error occurred while updating the event");
            }
        }

        // DELETE: api/bulletinevents/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            try
            {
                var deleted = await _bulletinEventService.DeleteEventAsync(id);

                if (!deleted)
                {
                    return NotFound($"Bulletin event with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bulletin event {Id}", id);
                return StatusCode(500, "An error occurred while deleting the event");
            }
        }
    }
}
