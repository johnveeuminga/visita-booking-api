using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VisitaBookingApi.Data;
using VisitaBookingApi.Models;
using VisitaBookingApi.Models.DTOs;
using VisitaBookingApi.Services.Interfaces;
using Google.Apis.Auth;

namespace VisitaBookingApi.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly visita_booking_api.Services.Interfaces.IEmailService _emailService;

        public AuthenticationService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger,
            visita_booking_api.Services.Interfaces.IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                // Find user by email
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null || !user.IsActive)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    };
                }

                // Verify password
                if (string.IsNullOrEmpty(user.PasswordHash) || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    };
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate tokens
                var accessToken = GenerateAccessToken(user);
                var refreshToken = await GenerateRefreshTokenAsync(user.Id);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Login successful.",
                    User = MapToUserDto(user),
                    AccessToken = accessToken.Token,
                    RefreshToken = refreshToken,
                    TokenExpiry = accessToken.Expiry
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login."
                };
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "User with this email already exists."
                    };
                }

                // Always assign Guest role for new registrations
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Guest");
                if (role == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Guest role not found. Please ensure roles are properly configured."
                    };
                }

                // Create user
                var user = new User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PasswordHash = HashPassword(request.Password),
                    Provider = "Local",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Assign role
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();

                // Reload user with roles
                user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstAsync(u => u.Id == user.Id);

                // Generate tokens
                var accessToken = GenerateAccessToken(user);
                var refreshToken = await GenerateRefreshTokenAsync(user.Id);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful.",
                    User = MapToUserDto(user),
                    AccessToken = accessToken.Token,
                    RefreshToken = refreshToken,
                    TokenExpiry = accessToken.Expiry
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during registration."
                };
            }
        }

        public async Task<AuthResponse> GoogleAuthAsync(GoogleAuthRequest request)
        {
            try
            {
                // Verify Google ID token
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
                
                if (payload == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid Google ID token."
                    };
                }

                // Check if user exists
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == payload.Email || u.ExternalId == payload.Subject);

                if (user == null)
                {
                    // Create new user
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
                    if (role == null)
                    {
                        return new AuthResponse
                        {
                            Success = false,
                            Message = "Invalid role specified."
                        };
                    }

                    user = new User
                    {
                        Email = payload.Email,
                        FirstName = payload.GivenName ?? "",
                        LastName = payload.FamilyName ?? "",
                        ExternalId = payload.Subject,
                        Provider = "Google",
                        IsEmailVerified = payload.EmailVerified,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Assign role
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        AssignedAt = DateTime.UtcNow
                    };

                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();

                    // Reload user with roles
                    user = await _context.Users
                        .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                        .FirstAsync(u => u.Id == user.Id);
                }
                else
                {
                    // Update existing user's Google info if needed
                    if (string.IsNullOrEmpty(user.ExternalId))
                    {
                        user.ExternalId = payload.Subject;
                    }
                    user.LastLoginAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                // Generate tokens
                var accessToken = GenerateAccessToken(user);
                var refreshToken = await GenerateRefreshTokenAsync(user.Id);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Google authentication successful.",
                    User = MapToUserDto(user),
                    AccessToken = accessToken.Token,
                    RefreshToken = refreshToken,
                    TokenExpiry = accessToken.Expiry
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google authentication");
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during Google authentication."
                };
            }
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                var refreshToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked);

                if (refreshToken == null || refreshToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token."
                    };
                }

                // Generate new tokens
                var accessToken = GenerateAccessToken(refreshToken.User);
                var newRefreshToken = await GenerateRefreshTokenAsync(refreshToken.User.Id);

                // Revoke old refresh token
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();

                return new AuthResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully.",
                    User = MapToUserDto(refreshToken.User),
                    AccessToken = accessToken.Token,
                    RefreshToken = newRefreshToken,
                    TokenExpiry = accessToken.Expiry
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during token refresh."
                };
            }
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                // Always return success response to avoid account enumeration
                if (user == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Password reset email sent if account exists.",
                        Data = true
                    };
                }

                // Generate secure token (URL-safe)
                var tokenBytes = RandomNumberGenerator.GetBytes(48);
                var token = Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');

                user.PasswordResetToken = token;
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // 1 hour expiry
                await _context.SaveChangesAsync();

                // Build reset link using frontend settings if available
                var frontendBase = _configuration["Frontend:BaseUrl"] ?? _configuration["Frontend:Url"];
                if (string.IsNullOrEmpty(frontendBase))
                {
                    // Fallback to an API route that the frontend could call
                    frontendBase = _configuration["App:BaseUrl"] ?? "https://your-frontend.example/reset-password";
                }

                // Ensure trailing slash handling
                var separator = frontendBase.EndsWith("/") ? string.Empty : "/";
                var resetLink = $"{frontendBase}{separator}auth/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

                // Send email (fire-and-forget best-effort)
                try
                {
                    await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Password reset email sent if account exists.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPasswordAsync for email {Email}", request.Email);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while processing the forgot password request.",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid token or email.",
                        Data = false
                    };
                }

                if (string.IsNullOrEmpty(user.PasswordResetToken) || user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Reset token is invalid or has expired.",
                        Data = false
                    };
                }

                if (user.PasswordResetToken != request.Token)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid reset token.",
                        Data = false
                    };
                }

                // Update password and clear reset token
                user.PasswordHash = HashPassword(request.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;

                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Password reset successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPasswordAsync for email {Email}", request.Email);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while resetting the password.",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<bool>> VerifyEmailAsync(string email, string token)
        {
            // Implementation for email verification
            // This is a placeholder
            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Email verified successfully.",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken)
        {
            try
            {
                var token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (token != null)
                {
                    token.IsRevoked = true;
                    await _context.SaveChangesAsync();
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Logged out successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred during logout.",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<bool>> RevokeAllTokensAsync(int userId)
        {
            try
            {
                var tokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                }

                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "All tokens revoked successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all tokens for user: {UserId}", userId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while revoking tokens.",
                    Data = false
                };
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

                if (user == null)
                {
                    return null;
                }

                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<visita_booking_api.Models.DTOs.PaginatedResponse<UserListItemDto>> GetUsersAsync(int page = 1, int pageSize = 20, string? email = null, string? name = null, string? role = null)
        {
            // Ensure sensible bounds
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var query = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .Where(u => u.IsActive);

            // Apply search filters
            if (!string.IsNullOrEmpty(email))
            {
                var e = email.Trim().ToLowerInvariant();
                query = query.Where(u => u.Email.ToLower().Contains(e));
            }

            if (!string.IsNullOrEmpty(name))
            {
                var n = name.Trim().ToLowerInvariant();
                query = query.Where(u => (u.FirstName + " " + u.LastName).ToLower().Contains(n) || u.FirstName.ToLower().Contains(n) || u.LastName.ToLower().Contains(n));
            }

            if (!string.IsNullOrEmpty(role))
            {
                var r = role.Trim();
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == r));
            }

            // Sort by CreatedAt desc (recent first)
            query = query.OrderByDescending(u => u.CreatedAt);

            var totalCount = await query.CountAsync();

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = users.Select(u => new UserListItemDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                IsActive = u.IsActive,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                CreatedAt = u.CreatedAt
            }).ToList();

            return visita_booking_api.Models.DTOs.PaginatedResponse<UserListItemDto>.Create(items, totalCount, page, pageSize);
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .ToListAsync();

            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        #region Private Methods

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        private (string Token, DateTime Expiry) GenerateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JWT");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "your-256-bit-secret");
            var expiry = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenExpiryMinutes"] ?? "60"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("provider", user.Provider)
            };

            // Add role claims
            foreach (var role in user.UserRoles.Select(ur => ur.Role.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return (tokenHandler.WriteToken(token), expiry);
        }

        private async Task<string> GenerateRefreshTokenAsync(int userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(int.Parse(_configuration["JWT:RefreshTokenExpiryDays"] ?? "30")),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken.Token;
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Provider = user.Provider,
                IsEmailVerified = user.IsEmailVerified,
                IsActive = user.IsActive,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        public async Task<ApiResponse<bool>> AssignRoleAsync(AssignRoleRequest request)
        {
            try
            {
                // Validate that the user exists
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId);
                
                if (user == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "User not found.",
                        Data = false
                    };
                }

                // Validate that the new role exists
                var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.NewRole);
                if (newRole == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid role specified.",
                        Data = false
                    };
                }

                // Check if user already has this role
                var existingUserRole = user.UserRoles.FirstOrDefault(ur => ur.Role.Name == request.NewRole);
                if (existingUserRole != null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"User already has the {request.NewRole} role.",
                        Data = false
                    };
                }

                // Assign the new role (allow multiple roles per user, but prevent duplicates)
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = newRole.Id,
                    AssignedAt = DateTime.UtcNow
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Role {NewRole} assigned to user {UserId} by admin", request.NewRole, request.UserId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"Successfully assigned {request.NewRole} role to user.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {Role} to user {UserId}", request.NewRole, request.UserId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while assigning the role.",
                    Data = false
                };
            }
        }

        #endregion
    }
}
