using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using visita_booking_api.Models.Entities;
using visita_booking_api.Services.Interfaces;
using VisitaBookingApi.Data;

namespace visita_booking_api.Controllers
{
    [ApiController]
    [Route("api/admin/establishments")]
    [Authorize(Roles = "Admin")]
    public class AdminEstablishmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IS3FileService _s3FileService;

        public AdminEstablishmentsController(
            ApplicationDbContext context,
            IS3FileService s3FileService
        )
        {
            _context = context;
            _s3FileService = s3FileService;
        }

        /// <summary>
        /// Admin: List all establishments with filters and pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] string? name,
            [FromQuery] string? status,
            [FromQuery] string? category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            if (page < 1)
                page = 1;
            if (pageSize < 1 || pageSize > 100)
                pageSize = 20;

            var query = _context.Establishments.Include(e => e.Owner).AsQueryable();

            // Filter by name
            if (!string.IsNullOrEmpty(name))
                query = query.Where(e => e.Name.Contains(name));

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                if (
                    Enum.TryParse(
                        typeof(Models.Enums.EstablishmentStatus),
                        status,
                        true,
                        out var parsed
                    )
                )
                {
                    var enumVal = (Models.Enums.EstablishmentStatus)parsed!;
                    query = query.Where(e => e.Status == enumVal);
                }
                else
                {
                    return BadRequest(new { success = false, message = "Invalid status value" });
                }
            }

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                if (
                    Enum.TryParse(
                        typeof(Models.Enums.EstablishmentCategory),
                        category,
                        true,
                        out var parsed
                    )
                )
                {
                    var enumVal = (Models.Enums.EstablishmentCategory)parsed!;
                    query = query.Where(e => e.Category == enumVal);
                }
                else
                {
                    return BadRequest(new { success = false, message = "Invalid category value" });
                }
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.Logo,
                    Category = e.Category.ToString(),
                    e.IsActive,
                    Status = e.Status.ToString(),
                    OwnerName = e.Owner.FullName,
                    OwnerEmail = e.Owner.Email,
                    CommentCount = _context.EstablishmentComments.Count(c =>
                        c.EstablishmentId == e.Id
                    ),
                    e.CreatedAt,
                })
                .ToListAsync();

            return Ok(
                new
                {
                    success = true,
                    data = items,
                    pagination = new
                    {
                        total = totalCount,
                        page,
                        pageSize,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    },
                }
            );
        }

        /// <summary>
        /// Admin: Get establishment details (with presigned business permit URL)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var establishment = await _context
                .Establishments.Include(e => e.Owner)
                .Include(e => e.Hours)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (establishment == null)
                return NotFound(new { success = false, message = "Establishment not found" });

            var response = new
            {
                establishment.Id,
                establishment.Name,
                establishment.Description,
                establishment.Logo,
                establishment.CoverImage,
                Category = establishment.Category.ToString(),
                establishment.Address,
                establishment.City,
                establishment.Latitude,
                establishment.Longitude,
                establishment.ContactNumber,
                establishment.Email,
                establishment.Website,
                establishment.FacebookPage,
                Status = establishment.Status.ToString(),
                establishment.IsActive,
                establishment.ApprovedAt,
                establishment.ApprovedById,
                establishment.RejectionReason,
                establishment.CreatedAt,
                establishment.UpdatedAt,
                establishment.OwnerId,
                OwnerName = establishment.Owner.FullName,
                OwnerEmail = establishment.Owner.Email,
                Hours = establishment.Hours?.Select(h => new
                {
                    h.Id,
                    DayOfWeek = h.DayOfWeek.ToString(),
                    OpenTime = h.OpenTime?.ToString(@"hh\:mm"),
                    CloseTime = h.CloseTime?.ToString(@"hh\:mm"),
                    h.IsClosed,
                }),
            };

            // Get presigned URL for business permit
            string? businessPermitUrl = null;
            if (!string.IsNullOrEmpty(establishment.BusinessPermitS3Key))
            {
                businessPermitUrl = await _s3FileService.GetPresignedUrlAsync(
                    establishment.BusinessPermitS3Key,
                    TimeSpan.FromMinutes(15)
                );
            }

            return Ok(
                new
                {
                    success = true,
                    data = response,
                    businessPermitUrl,
                }
            );
        }

        /// <summary>
        /// Admin: Approve establishment
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e => e.Id == id);
            if (establishment == null)
                return NotFound(new { success = false, message = "Establishment not found" });

            var adminId = GetCurrentUserId();
            if (!adminId.HasValue)
                return Forbid();

            establishment.Approve(adminId.Value);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Establishment approved successfully" });
        }

        /// <summary>
        /// Admin: Reject establishment
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(
            int id,
            [FromBody] RejectEstablishmentRequest request
        )
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e => e.Id == id);
            if (establishment == null)
                return NotFound(new { success = false, message = "Establishment not found" });

            establishment.Reject(request.Reason ?? "No reason provided");
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Establishment rejected" });
        }

        /// <summary>
        /// Admin: Set establishment to pending status
        /// </summary>
        [HttpPost("{id}/set-pending")]
        public async Task<IActionResult> SetPending(int id, [FromBody] SetPendingRequest? request)
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e => e.Id == id);
            if (establishment == null)
                return NotFound(new { success = false, message = "Establishment not found" });

            var adminId = GetCurrentUserId();
            if (!adminId.HasValue)
                return Forbid();

            // Set status to pending
            establishment.Status = Models.Enums.EstablishmentStatus.Pending;
            establishment.UpdateTimestamp();

            // Add comment if provided
            if (!string.IsNullOrWhiteSpace(request?.Comment))
            {
                var comment = new EstablishmentComment
                {
                    EstablishmentId = id,
                    AdminId = adminId.Value,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.EstablishmentComments.Add(comment);
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Establishment set to pending" });
        }

        /// <summary>
        /// Admin: Add internal comment/note
        /// </summary>
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Comment))
                return BadRequest(new { success = false, message = "Comment cannot be empty" });

            var establishment = await _context.Establishments.FirstOrDefaultAsync(e => e.Id == id);
            if (establishment == null)
                return NotFound(new { success = false, message = "Establishment not found" });

            var adminId = GetCurrentUserId();
            if (!adminId.HasValue)
                return Forbid();

            var comment = new EstablishmentComment
            {
                EstablishmentId = id,
                AdminId = adminId.Value,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
            };

            _context.EstablishmentComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Comment added successfully" });
        }

        /// <summary>
        /// Admin: Get all internal comments for establishment
        /// </summary>
        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComments(int id)
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e => e.Id == id);
            if (establishment == null)
                return NotFound(new { success = false, message = "Establishment not found" });

            var comments = await _context
                .EstablishmentComments.Include(c => c.Admin)
                .Where(c => c.EstablishmentId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.EstablishmentId,
                    c.AdminId,
                    AdminName = c.Admin.FullName ?? c.Admin.Email,
                    c.Comment,
                    c.CreatedAt,
                })
                .ToListAsync();

            return Ok(new { success = true, data = comments });
        }

        /// <summary>
        /// Admin: Delete establishment (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var establishment = await _context.Establishments.FirstOrDefaultAsync(e => e.Id == id);
            if (establishment == null)
                return NotFound(new { success = false, message = "Establishment not found" });

            // Delete logo if exists
            if (!string.IsNullOrEmpty(establishment.Logo))
            {
                var key = _s3FileService.ExtractS3KeyFromUrl(establishment.Logo);
                if (!string.IsNullOrEmpty(key))
                    await _s3FileService.DeleteFileAsync(key);
            }

            establishment.IsActive = false;
            establishment.UpdateTimestamp();
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        // Request DTOs
        public class RejectEstablishmentRequest
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
