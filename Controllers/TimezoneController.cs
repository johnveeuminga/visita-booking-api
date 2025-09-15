using Microsoft.AspNetCore.Mvc;
using visita_booking_api.Services;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimezoneController : ControllerBase
    {
        private readonly ITimezoneService _timezoneService;

        public TimezoneController(ITimezoneService timezoneService)
        {
            _timezoneService = timezoneService;
        }

        [HttpGet("info")]
        public IActionResult GetTimezoneInfo()
        {
            return Ok(new
            {
                ServerUtcTime = DateTime.UtcNow,
                GmtPlus8Time = _timezoneService.Now,
                GmtPlus8Today = _timezoneService.Today,
                TimezoneName = _timezoneService.LocalTimeZone.DisplayName,
                TimezoneId = _timezoneService.LocalTimeZone.Id,
                UtcOffset = _timezoneService.LocalTimeZone.BaseUtcOffset.ToString()
            });
        }

        [HttpGet("convert")]
        public IActionResult ConvertTime([FromQuery] DateTime utcTime)
        {
            var localTime = _timezoneService.ConvertToLocalTime(utcTime);
            var backToUtc = _timezoneService.ConvertToUtc(localTime);

            return Ok(new
            {
                OriginalUtc = utcTime,
                ConvertedToGmtPlus8 = localTime,
                ConvertedBackToUtc = backToUtc
            });
        }
    }
}