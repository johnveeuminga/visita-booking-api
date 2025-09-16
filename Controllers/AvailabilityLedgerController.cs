using Microsoft.AspNetCore.Mvc;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/ledger")]
    public class AvailabilityLedgerController : ControllerBase
    {
        private readonly IAvailabilityLedgerService _ledgerService;

        public AvailabilityLedgerController(IAvailabilityLedgerService ledgerService)
        {
            _ledgerService = ledgerService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (end <= start) return BadRequest("end must be after start");
            var count = await _ledgerService.GenerateLedgerAsync(start, end);
            return Ok(new { processedRooms = count });
        }

        [HttpPost("warmup")]
        public async Task<IActionResult> Warmup([FromQuery] int roomId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (end <= start) return BadRequest("end must be after start");
            var ok = await _ledgerService.WarmupRoomLedgerAsync(roomId, start, end);
            return ok ? Ok() : NotFound();
        }
    }
}
