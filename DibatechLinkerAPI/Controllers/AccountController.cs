using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DibatechLinkerAPI.Models.DTOs;
using DibatechLinkerAPI.Services.Interfaces;
using System.Security.Claims;

namespace DibatechLinkerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Register([FromBody] RegisterDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.RegisterAsync(request);

                if (result == null)
                {
                    return BadRequest(new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Registration failed. Email may already be in use."
                    });
                }

                return Ok(new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "User registered successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Internal server error during registration"
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login([FromBody] LoginDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.LoginAsync(request);

                if (result == null)
                {
                    return Unauthorized(new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                return Ok(new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "User logged in successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Internal server error during login"
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.RefreshTokenAsync(request.RefreshToken);

                if (result == null)
                {
                    return Unauthorized(new ApiResponseDto<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    });
                }

                return Ok(new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = "Internal server error during token refresh"
                });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponseDto<bool>>> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var success = await _authService.LogoutAsync(userId);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = success,
                    Message = success ? "User logged out successfully" : "Logout failed",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user logout");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Internal server error during logout"
                });
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponseDto<UserProfileDto>>> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponseDto<UserProfileDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var profile = await _authService.GetUserProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new ApiResponseDto<UserProfileDto>
                    {
                        Success = false,
                        Message = "User profile not found"
                    });
                }

                return Ok(new ApiResponseDto<UserProfileDto>
                {
                    Success = true,
                    Message = "Profile retrieved successfully",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, new ApiResponseDto<UserProfileDto>
                {
                    Success = false,
                    Message = "Internal server error while retrieving profile"
                });
            }
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var success = await _authService.UpdateUserProfileAsync(userId, request);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = success,
                    Message = success ? "Profile updated successfully" : "Profile update failed",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Internal server error while updating profile"
                });
            }
        }
    }
}
