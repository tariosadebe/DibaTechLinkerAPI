using DibatechLinkerAPI.Models.Domain;
using DibatechLinkerAPI.Models.DTOs;

namespace DibatechLinkerAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto request);
        Task<AuthResponseDto?> LoginAsync(LoginDto request);
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string userId);
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto request);
        string GenerateJwtToken(ApplicationUser user);
        string GenerateRefreshToken();
    }
}
