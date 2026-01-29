using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VisitaBookingAPI.DTOs;
using VisitaBookingAPI.Services.Interfaces;

namespace VisitaBookingAPI.Controllers
{
    [ApiController]
    [Route("api/establishments")]
    public class EstablishmentsController : ControllerBase
    {
        private readonly IEstablishmentService _establishmentService;

        public EstablishmentsController(IEstablishmentService establishmentService)
        {
            _establishmentService = establishmentService;
        }

        [HttpGet]
        public async Task<ActionResult<EstablishmentSearchResult>> SearchEstablishments(
            [FromQuery] EstablishmentSearchParams searchParams
        )
        {
            searchParams.Status = "Approved";
            searchParams.IsActive = true;
            var result = await _establishmentService.SearchEstablishmentsAsync(searchParams);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EstablishmentDetailDto>> GetEstablishment(int id)
        {
            var establishment = await _establishmentService.GetEstablishmentByIdAsync(id);
            if (establishment == null || establishment.Status != "Approved")
            {
                return NotFound(new { message = "Establishment not found" });
            }
            return Ok(establishment);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<EstablishmentCategoryDto>>> GetCategories()
        {
            var categories = await _establishmentService.GetCategoriesWithSubcategoriesAsync();
            return Ok(categories);
        }
    }
}
