using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DibatechLinkerAPI.Models.Domain;
using DibatechLinkerAPI.Models.DTOs;
using DibatechLinkerAPI.Services.Interfaces;

namespace DibatechLinkerAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService; // Inject IEmailService

        private const string SendGridFromEmailKey = "SendGrid:FromEmail";
        private const string SendGridFromNameKey = "SendGrid:FromName";

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            IEmailService emailService) // Add emailService to constructor
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService; // Initialize emailService
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto request)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                    return null;
                }

                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true // For simplicity, auto-confirm emails
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("User registration failed for {Email}: {Errors}", 
                        request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return null;
                }

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken ?? string.Empty;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                // Send welcome email after successful registration
                try
                {
                    await _emailService.SendWelcomeEmailAsync(request.Email, $"{request.FirstName} {request.LastName}");
                    _logger.LogInformation("Welcome email sent to {Email}", request.Email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send welcome email to {Email}", request.Email);
                    // Don't fail registration if email fails
                }

                return new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = new UserProfileDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CreatedAt = user.CreatedAt,
                        ReminderFrequency = user.ReminderFrequency.ToString(),
                        PreferredReminderDay = user.PreferredReminderDay,
                        PreferredReminderTime = user.PreferredReminderTime?.ToString("HH:mm"),
                        IsEmailNotificationEnabled = user.IsEmailNotificationEnabled,
                        IsPushNotificationEnabled = user.IsPushNotificationEnabled
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for {Email}", request.Email);
                return null;
            }
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                    return null;
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed login attempt for {Email}", request.Email);
                    return null;
                }

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken ?? string.Empty;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = new UserProfileDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt,
                        ReminderFrequency = user.ReminderFrequency.ToString(),
                        PreferredReminderDay = user.PreferredReminderDay,
                        PreferredReminderTime = user.PreferredReminderTime?.ToString("HH:mm"),
                        IsEmailNotificationEnabled = user.IsEmailNotificationEnabled,
                        IsPushNotificationEnabled = user.IsPushNotificationEnabled
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", request.Email);
                return null;
            }
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

                if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Invalid or expired refresh token");
                    return null;
                }

                var newToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken ?? string.Empty;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                return new AuthResponseDto
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = new UserProfileDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt,
                        ReminderFrequency = user.ReminderFrequency.ToString(),
                        PreferredReminderDay = user.PreferredReminderDay,
                        PreferredReminderTime = user.PreferredReminderTime?.ToString("HH:mm"),
                        IsEmailNotificationEnabled = user.IsEmailNotificationEnabled,
                        IsPushNotificationEnabled = user.IsPushNotificationEnabled
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return null;
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", userId);
                return false;
            }
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return null;

                return new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    ReminderFrequency = user.ReminderFrequency.ToString(),
                    PreferredReminderDay = user.PreferredReminderDay,
                    PreferredReminderTime = user.PreferredReminderTime?.ToString("HH:mm"),
                    IsEmailNotificationEnabled = user.IsEmailNotificationEnabled,
                    IsPushNotificationEnabled = user.IsPushNotificationEnabled
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                if (request.FirstName != null)
                    user.FirstName = request.FirstName;

                if (request.LastName != null)
                    user.LastName = request.LastName;

                if (!string.IsNullOrEmpty(request.ReminderFrequency) && 
                    Enum.TryParse<ReminderFrequency>(request.ReminderFrequency, true, out var frequency))
                {
                    user.ReminderFrequency = frequency;
                }

                if (request.PreferredReminderDay.HasValue)
                    user.PreferredReminderDay = request.PreferredReminderDay.Value;

                if (!string.IsNullOrEmpty(request.PreferredReminderTime) && 
                    TimeOnly.TryParse(request.PreferredReminderTime, out var time))
                {
                    user.PreferredReminderTime = time;
                }

                if (request.IsEmailNotificationEnabled.HasValue)
                    user.IsEmailNotificationEnabled = request.IsEmailNotificationEnabled.Value;

                if (request.IsPushNotificationEnabled.HasValue)
                    user.IsPushNotificationEnabled = request.IsPushNotificationEnabled.Value;

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile for {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal if email exists or not for security
                    _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
                    return true;
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // URL encode the token for safe transmission
                var encodedToken = Uri.EscapeDataString(resetToken);
                
                // Send password reset email
                var emailSent = await _emailService.SendPasswordResetEmailAsync(email, encodedToken);
                
                if (emailSent)
                {
                    _logger.LogInformation("Password reset email sent to {Email}", email);
                }
                else
                {
                    _logger.LogWarning("Failed to send password reset email to {Email}", email);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting password reset for {Email}", email);
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Password reset attempted for non-existent email: {Email}", email);
                    return false;
                }

                // URL decode the token
                var decodedToken = Uri.UnescapeDataString(token);
                
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset successful for {Email}", email);
                    
                    // Update last login time
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                    
                    return true;
                }

                _logger.LogWarning("Password reset failed for {Email}. Errors: {Errors}", 
                    email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", email);
                return false;
            }
        }

        public string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
                new Claim("firstName", user.FirstName ?? ""),
                new Claim("lastName", user.LastName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[64];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
