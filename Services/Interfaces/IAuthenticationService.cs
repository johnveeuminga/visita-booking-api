using VisitaBookingApi.Models.DTOs;

namespace VisitaBookingApi.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> GoogleAuthAsync(GoogleAuthRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ApiResponse<bool>> VerifyEmailAsync(string email, string token);
        Task<ApiResponse<bool>> LogoutAsync(string refreshToken);
        Task<ApiResponse<bool>> RevokeAllTokensAsync(int userId);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<ApiResponse<bool>> AssignRoleAsync(AssignRoleRequest request);
        Task<visita_booking_api.Models.DTOs.PaginatedResponse<UserListItemDto>> GetUsersAsync(
            int page = 1,
            int pageSize = 20,
            string? q = null,
            string? role = null
        );
        Task<List<RoleDto>> GetAllRolesAsync();
    }
}
