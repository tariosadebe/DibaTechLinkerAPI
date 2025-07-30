using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DibatechLinkerAPI.Models.DTOs;
using DibatechLinkerAPI.Services.Interfaces;
using System.Security.Claims;

namespace DibatechLinkerAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RemindersController : ControllerBase
    {
        private readonly IReminderService _reminderService;
        private readonly ILogger<RemindersController> _logger;

        public RemindersController(IReminderService reminderService, ILogger<RemindersController> logger)
        {
            _reminderService = reminderService;
            _logger = logger;
        }

        [HttpPost("subscribe")]
        public async Task<ActionResult<ApiResponseDto<bool>>> Subscribe([FromBody] ReminderSubscriptionDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var success = await _reminderService.SubscribeToRemindersAsync(userId, request);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = success,
                    Message = success ? "Successfully subscribed to reminders" : "Failed to subscribe to reminders",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to reminders");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Internal server error while subscribing to reminders"
                });
            }
        }

        [HttpGet("status")]
        public async Task<ActionResult<ApiResponseDto<ReminderStatusDto>>> GetStatus()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponseDto<ReminderStatusDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var status = await _reminderService.GetReminderStatusAsync(userId);

                if (status == null)
                {
                    return NotFound(new ApiResponseDto<ReminderStatusDto>
                    {
                        Success = false,
                        Message = "No reminder subscription found"
                    });
                }

                return Ok(new ApiResponseDto<ReminderStatusDto>
                {
                    Success = true,
                    Message = "Reminder status retrieved successfully",
                    Data = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reminder status");
                return StatusCode(500, new ApiResponseDto<ReminderStatusDto>
                {
                    Success = false,
                    Message = "Internal server error while retrieving reminder status"
                });
            }
        }

        [HttpPost("unsubscribe")]
        public async Task<ActionResult<ApiResponseDto<bool>>> Unsubscribe()
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

                var success = await _reminderService.UnsubscribeFromRemindersAsync(userId);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = success,
                    Message = success ? "Successfully unsubscribed from reminders" : "Failed to unsubscribe from reminders",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from reminders");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Internal server error while unsubscribing from reminders"
                });
            }
        }
    }
}
