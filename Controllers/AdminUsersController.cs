using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisitaBookingApi.Models.DTOs;
using VisitaBookingApi.Services.Interfaces;

namespace VisitaBookingApi.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(IAuthenticationService authService, ILogger<AdminUsersController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated users sorted by registration (most recent first)
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(typeof(visita_booking_api.Models.DTOs.PaginatedResponse<UserListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<visita_booking_api.Models.DTOs.PaginatedResponse<UserListItemDto>>> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? q = null,
            [FromQuery] string? role = null)
        {
            var result = await _authService.GetUsersAsync(pageNumber, pageSize, q, role);
            return Ok(result);
        }

        /// <summary>
        /// Get all available roles
        /// </summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RoleDto>>> GetRoles()
        {
            var roles = await _authService.GetAllRolesAsync();
            return Ok(roles);
        }

        /// Assign a role to a user
        /// </summary>
        [HttpPost("assign-role")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<bool>>> AssignRole([FromBody] AssignRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid request data.",
                    Data = false
                });
            }

            var result = await _authService.AssignRoleAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

/// <summary>
/// Remove a role from a user
/// </summary>
[HttpPost("remove-role")]
[ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
public async Task<ActionResult<ApiResponse<bool>>> RemoveRole([FromBody] RemoveRoleRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new ApiResponse<bool>
        {
            Success = false,
            Message = "Invalid request data.",
            Data = false
        });
    }

    var result = await _authService.RemoveRoleAsync(request);
    
    if (!result.Success)
    {
        return BadRequest(result);
    }

    return Ok(result);
}
    }
    
}
