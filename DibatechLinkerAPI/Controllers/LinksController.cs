using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DibatechLinkerAPI.Data;
using DibatechLinkerAPI.Models.DTOs;
using DibatechLinkerAPI.Services.Interfaces;
using System.Security.Claims;

namespace DibatechLinkerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILinkParsingService _linkParsingService;
        private readonly ISavedLinkService _savedLinkService;
        private readonly ILogger<LinksController> _logger;

        public LinksController(
            ApplicationDbContext context,
            ILinkParsingService linkParsingService,
            ISavedLinkService savedLinkService,
            ILogger<LinksController> logger)
        {
            _context = context;
            _linkParsingService = linkParsingService;
            _savedLinkService = savedLinkService;
            _logger = logger;
        }

        [HttpPost("parse")]
        public async Task<ActionResult<ApiResponseDto<ParsedLinkDto>>> ParseLink([FromBody] ParseLinkRequestDto request)
        {
            try
            {
                var parsedLink = await _linkParsingService.ParseLinkAsync(request.Url);
                
                // Check if this URL has been parsed before
                var existingParsedLink = await _context.ParsedLinks
                    .FirstOrDefaultAsync(pl => pl.OriginalUrl == request.Url);

                if (existingParsedLink != null)
                {
                    parsedLink = existingParsedLink;
                }
                else
                {
                    _context.ParsedLinks.Add(parsedLink);
                    await _context.SaveChangesAsync();
                }

                var result = new ParsedLinkDto
                {
                    Id = parsedLink.Id,
                    OriginalUrl = parsedLink.OriginalUrl,
                    Title = parsedLink.Title,
                    Description = parsedLink.Description,
                    ImageUrl = parsedLink.ImageUrl,
                    Author = parsedLink.Author,
                    SiteName = parsedLink.SiteName,
                    Domain = parsedLink.Domain,
                    ContentType = parsedLink.ContentType.ToString(),
                    Category = parsedLink.Category.ToString(),
                    ParsedAt = parsedLink.ParsedAt,
                    IsValidUrl = parsedLink.IsValidUrl,
                    ErrorMessage = parsedLink.ErrorMessage
                };

                return Ok(new ApiResponseDto<ParsedLinkDto>
                {
                    Success = true,
                    Message = "Link parsed successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing link: {Url}", request.Url);
                return BadRequest(new ApiResponseDto<ParsedLinkDto>
                {
                    Success = false,
                    Message = "Failed to parse link",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("save")]
        public async Task<ActionResult<ApiResponseDto<SavedLinkDto>>> SaveLink([FromBody] SaveLinkRequestDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionId = HttpContext.Session.Id;

                var savedLink = await _savedLinkService.SaveLinkAsync(
                    request.ParsedLinkId, userId, sessionId, request);

                var result = await _savedLinkService.GetSavedLinkByIdAsync(savedLink.Id, userId, sessionId);

                return Ok(new ApiResponseDto<SavedLinkDto>
                {
                    Success = true,
                    Message = "Link saved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving link: {ParsedLinkId}", request.ParsedLinkId);
                return BadRequest(new ApiResponseDto<SavedLinkDto>
                {
                    Success = false,
                    Message = "Failed to save link",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("mine")]
        public async Task<ActionResult<ApiResponseDto<PaginatedResponseDto<SavedLinkDto>>>> GetMySavedLinks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? category = null,
            [FromQuery] string? status = null,
            [FromQuery] int? folderId = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionId = HttpContext.Session.Id;

                var result = await _savedLinkService.GetUserSavedLinksAsync(
                    userId, sessionId, pageNumber, pageSize, category, status, folderId);

                return Ok(new ApiResponseDto<PaginatedResponseDto<SavedLinkDto>>
                {
                    Success = true,
                    Message = "Saved links retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved links");
                return BadRequest(new ApiResponseDto<PaginatedResponseDto<SavedLinkDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve saved links",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<SavedLinkDto>>> GetSavedLink(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionId = HttpContext.Session.Id;

                var result = await _savedLinkService.GetSavedLinkByIdAsync(id, userId, sessionId);

                if (result == null)
                {
                    return NotFound(new ApiResponseDto<SavedLinkDto>
                    {
                        Success = false,
                        Message = "Saved link not found"
                    });
                }

                return Ok(new ApiResponseDto<SavedLinkDto>
                {
                    Success = true,
                    Message = "Saved link retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved link: {Id}", id);
                return BadRequest(new ApiResponseDto<SavedLinkDto>
                {
                    Success = false,
                    Message = "Failed to retrieve saved link",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("share/{shareToken}")]
        public async Task<ActionResult<ApiResponseDto<SavedLinkDto>>> GetSharedLink(string shareToken)
        {
            try
            {
                var result = await _savedLinkService.GetPublicSharedLinkAsync(shareToken);

                if (result == null)
                {
                    return NotFound(new ApiResponseDto<SavedLinkDto>
                    {
                        Success = false,
                        Message = "Shared link not found or expired"
                    });
                }

                return Ok(new ApiResponseDto<SavedLinkDto>
                {
                    Success = true,
                    Message = "Shared link retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shared link: {ShareToken}", shareToken);
                return BadRequest(new ApiResponseDto<SavedLinkDto>
                {
                    Success = false,
                    Message = "Failed to retrieve shared link",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateSavedLink(int id, [FromBody] UpdateSavedLinkDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionId = HttpContext.Session.Id;

                var success = await _savedLinkService.UpdateSavedLinkAsync(id, userId, sessionId, request);

                if (!success)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Saved link not found"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Saved link updated successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating saved link: {Id}", id);
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Failed to update saved link",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteSavedLink(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionId = HttpContext.Session.Id;

                var success = await _savedLinkService.DeleteSavedLinkAsync(id, userId, sessionId);

                if (!success)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Saved link not found"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Saved link deleted successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting saved link: {Id}", id);
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Failed to delete saved link",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("{id}/share")]
        public async Task<ActionResult<ApiResponseDto<string>>> GenerateShareLink(int id, [FromBody] ShareLinkRequestDto? request = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionId = HttpContext.Session.Id;

                var shareToken = await _savedLinkService.GenerateShareTokenAsync(
                    id, userId, sessionId, request?.ExpiryDate);

                return Ok(new ApiResponseDto<string>
                {
                    Success = true,
                    Message = "Share link generated successfully",
                    Data = shareToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating share link: {Id}", id);
                return BadRequest(new ApiResponseDto<string>
                {
                    Success = false,
                    Message = "Failed to generate share link",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("{id}/mark-read")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAsRead(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionId = HttpContext.Session.Id;

                var success = await _savedLinkService.MarkAsReadAsync(id, userId, sessionId);

                if (!success)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Saved link not found"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Link marked as read",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking link as read: {Id}", id);
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Failed to mark link as read",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("{id}/toggle-favourite")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ToggleFavourite(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionId = HttpContext.Session.Id;

                var success = await _savedLinkService.ToggleFavouriteAsync(id, userId, sessionId);

                if (!success)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Saved link not found"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Favourite status toggled",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favourite: {Id}", id);
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Failed to toggle favourite",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }

    public class ShareLinkRequestDto
    {
        public DateTime? ExpiryDate { get; set; }
    }
}
