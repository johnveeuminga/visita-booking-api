using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using visita_booking_api.Models.DTOs;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/admin/accommodations")]
    [Authorize(Roles = "Admin")]
    public class AdminAccommodationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IS3FileService _s3FileService;

        public AdminAccommodationsController(ApplicationDbContext context, IS3FileService s3FileService)
        {
            _context = context;
            _s3FileService = s3FileService;
        }

        /// <summary>
        /// Admin: search and list accommodations (filter by name and status). Paginated.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<AccommodationSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<AccommodationSummaryDto>>> List([FromQuery] string? name, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Accommodations
                .Include(a => a.Rooms)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(a => a.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse(typeof(Models.Enums.AccommodationStatus), status, true, out var parsed))
                {
                    var enumVal = (Models.Enums.AccommodationStatus)parsed!;
                    query = query.Where(a => a.Status == enumVal);
                }
                else
                {
                    return BadRequest("Invalid status value");
                }
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AccommodationSummaryDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Logo = a.Logo,
                    IsActive = a.IsActive,
                    Status = a.Status.ToString(),
                    ActiveRoomCount = a.Rooms.Count(r => r.IsActive),
                    CommentCount = _context.AccommodationComments.Count(c => c.AccommodationId == a.Id) ,
                })
                .ToListAsync();

            var response = PaginatedResponse<AccommodationSummaryDto>.Create(items, totalCount, page, pageSize);

            return Ok(response);
        }

        /// <summary>
        /// Admin: get accommodation details (includes presigned URLs for private docs)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AccommodationResponseDto>> Get(int id)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.Owner)
                .Include(a => a.Rooms)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (accommodation == null) return NotFound();

            var response = new AccommodationResponseDto
            {
                Id = accommodation.Id,
                Name = accommodation.Name,
                Description = accommodation.Description,
                Logo = accommodation.Logo,
                IsActive = accommodation.IsActive,
                Status = accommodation.Status.ToString(),
                ApprovedAt = accommodation.ApprovedAt,
                ApprovedById = accommodation.ApprovedById,
                RejectionReason = accommodation.RejectionReason,
                CreatedAt = accommodation.CreatedAt,
                UpdatedAt = accommodation.UpdatedAt,
                OwnerId = accommodation.OwnerId,
                OwnerName = accommodation.Owner?.FullName ?? "Unknown",
                OwnerEmail = accommodation.Owner?.Email ?? "Unknown",
                ActiveRoomCount = accommodation.Rooms.Count(r => r.IsActive)
            };

            var presignExpiration = TimeSpan.FromMinutes(15);
            if (!string.IsNullOrEmpty(accommodation.BusinessPermitS3Key))
            {
                response.BusinessPermitUrl = await _s3FileService.GetPresignedUrlAsync(accommodation.BusinessPermitS3Key, presignExpiration);
            }
            if (!string.IsNullOrEmpty(accommodation.DotAccreditationS3Key))
            {
                response.DotAccreditationUrl = await _s3FileService.GetPresignedUrlAsync(accommodation.DotAccreditationS3Key, presignExpiration);
            }

            return Ok(response);
        }

        /// <summary>
        /// Admin: delete accommodation (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var accommodation = await _context.Accommodations
                .FirstOrDefaultAsync(a => a.Id == id);

            if (accommodation == null) return NotFound();

            // Check if accommodation has active rooms
            var hasActiveRooms = await _context.Rooms.AnyAsync(r => r.AccommodationId == id && r.IsActive);
            if (hasActiveRooms)
            {
                return BadRequest("Cannot delete accommodation with active rooms. Deactivate rooms first.");
            }

            // delete logo if exists
            if (!string.IsNullOrEmpty(accommodation.Logo))
            {
                var key = _s3FileService.ExtractS3KeyFromUrl(accommodation.Logo);
                if (!string.IsNullOrEmpty(key)) await _s3FileService.DeleteFileAsync(key);
            }

            accommodation.IsActive = false;
            accommodation.UpdateTimestamp();
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Admin: approve an accommodation
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var accommodation = await _context.Accommodations.FirstOrDefaultAsync(a => a.Id == id);
            if (accommodation == null) return NotFound();

            var adminId = GetCurrentUserId();
            if (!adminId.HasValue) return Forbid();

            accommodation.Approve(adminId.Value);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true });
        }

        /// <summary>
        /// Admin: reject an accommodation with reason
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectAccommodationRequest request)
        {
            var accommodation = await _context.Accommodations.FirstOrDefaultAsync(a => a.Id == id);
            if (accommodation == null) return NotFound();

            accommodation.Reject(request.Reason ?? string.Empty);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true });
        }



/// <summary>
        /// Admin: set accommodation to pending status with optional internal comment
        /// </summary>
        [HttpPost("{id}/set-pending")]
        public async Task<IActionResult> SetPending(int id, [FromBody] SetPendingRequest? request)
        {
            var accommodation = await _context.Accommodations.FirstOrDefaultAsync(a => a.Id == id);
            if (accommodation == null) return NotFound();

            var adminId = GetCurrentUserId();
            if (!adminId.HasValue) return Forbid();

            // Set status to pending
            accommodation.Status = Models.Enums.AccommodationStatus.Pending;
            accommodation.UpdateTimestamp();

            // If comment is provided, add it as an internal note
            if (!string.IsNullOrWhiteSpace(request?.Comment))
            {
                var comment = new AccommodationComment
                {
                    AccommodationId = id,
                    AdminId = adminId.Value,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AccommodationComments.Add(comment);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true });
        }

        /// <summary>
        /// Admin: add an internal comment/note to an accommodation
        /// These comments are only visible to other admins, not to the accommodation owner
        /// </summary>
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return BadRequest("Comment cannot be empty");
            }

            var accommodation = await _context.Accommodations.FirstOrDefaultAsync(a => a.Id == id);
            if (accommodation == null) return NotFound();

            var adminId = GetCurrentUserId();
            if (!adminId.HasValue) return Forbid();

            var comment = new AccommodationComment
            {
                AccommodationId = id,
                AdminId = adminId.Value,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.AccommodationComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true });
        }

        /// <summary>
        /// Admin: get all internal comments for an accommodation
        /// These are internal admin notes not visible to accommodation owners
        /// </summary>
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<List<AccommodationCommentDto>>> GetComments(int id)
        {
            var accommodation = await _context.Accommodations.FirstOrDefaultAsync(a => a.Id == id);
            if (accommodation == null) return NotFound();

            var comments = await _context.AccommodationComments
                .Include(c => c.Admin)
                .Where(c => c.AccommodationId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new AccommodationCommentDto
                {
                    Id = c.Id,
                    AccommodationId = c.AccommodationId,
                    AdminId = c.AdminId,
                    AdminName = c.Admin.FullName ?? c.Admin.Email,
                    Comment = c.Comment,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }

         private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        public class RejectAccommodationRequest
        {
            public string? Reason { get; set; }
        }
        public class SetPendingRequest
        {
            public string? Comment { get; set; }
        }

        public class AddCommentRequest
        {
            public string Comment { get; set; } = string.Empty;
        }
    
    }
}
