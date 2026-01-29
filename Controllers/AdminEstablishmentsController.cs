using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisitaBookingAPI.DTOs;
using VisitaBookingAPI.Services.Interfaces;

namespace VisitaBookingAPI.Controllers
{
    [ApiController]
    [Route("api/admin/establishments")]
    [Authorize(Roles = "Admin")]
    public class AdminEstablishmentsController : ControllerBase
    {
        private readonly IEstablishmentService _establishmentService;

        public AdminEstablishmentsController(IEstablishmentService establishmentService)
        {
            _establishmentService = establishmentService;
        }

        [HttpGet("pending")]
        public async Task<ActionResult<List<AdminEstablishmentListDto>>> GetPendingEstablishments()
        {
            var establishments = await _establishmentService.GetPendingEstablishmentsAsync();
            return Ok(establishments);
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminEstablishmentListDto>>> GetAllEstablishments(
            [FromQuery] string? status = null,
            [FromQuery] string? category = null
        )
        {
            var establishments = await _establishmentService.GetAllEstablishmentsAsync(
                status,
                category
            );
            return Ok(establishments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EstablishmentDetailDto>> GetEstablishment(int id)
        {
            var establishment = await _establishmentService.GetEstablishmentByIdAsync(id);
            if (establishment == null)
            {
                return NotFound(new { message = "Establishment not found" });
            }
            return Ok(establishment);
        }

        [HttpPost("{id}/approve")]
        public async Task<ActionResult> ApproveEstablishment(int id)
        {
            var adminId = GetCurrentUserId();
            var success = await _establishmentService.ApproveEstablishmentAsync(id, adminId);
            if (!success)
            {
                return NotFound(new { message = "Establishment not found" });
            }
            return Ok(new { message = "Establishment approved successfully" });
        }

        [HttpPost("{id}/reject")]
        public async Task<ActionResult> RejectEstablishment(
            int id,
            [FromBody] RejectEstablishmentDto dto
        )
        {
            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            {
                return BadRequest(new { message = "Rejection reason is required" });
            }

            var adminId = GetCurrentUserId();
            var success = await _establishmentService.RejectEstablishmentAsync(
                id,
                adminId,
                dto.RejectionReason
            );
            if (!success)
            {
                return NotFound(new { message = "Establishment not found" });
            }
            return Ok(new { message = "Establishment rejected", reason = dto.RejectionReason });
        }

        [HttpGet("{id}/business-permit")]
        public async Task<ActionResult> GetBusinessPermit(int id)
        {
            var url = await _establishmentService.GetBusinessPermitDownloadUrlAsync(id);
            if (url == null)
            {
                return NotFound(new { message = "Business permit not found" });
            }
            return Ok(new { url });
        }

        [HttpGet("statistics")]
        public async Task<ActionResult> GetStatistics()
        {
            var pending = await _establishmentService.GetPendingEstablishmentsAsync();
            var all = await _establishmentService.GetAllEstablishmentsAsync(null, null);

            var stats = new
            {
                totalEstablishments = all.Count,
                pendingApprovals = pending.Count,
                approved = all.Count(e => e.Status == "Approved"),
                rejected = all.Count(e => e.Status == "Rejected"),
                byCategory = all.GroupBy(e => e.Category)
                    .Select(g => new { category = g.Key, count = g.Count() })
                    .ToList(),
            };

            return Ok(stats);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return int.Parse(userIdClaim);
        }
    }
}
