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
    public class FoldersController : ControllerBase
    {
        private readonly IFolderService _folderService;
        private readonly ILogger<FoldersController> _logger;

        public FoldersController(IFolderService folderService, ILogger<FoldersController> logger)
        {
            _folderService = folderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<UserFolderDto>>>> GetFolders()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponseDto<List<UserFolderDto>>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var folders = await _folderService.GetUserFoldersAsync(userId);

                return Ok(new ApiResponseDto<List<UserFolderDto>>
                {
                    Success = true,
                    Message = "Folders retrieved successfully",
                    Data = folders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user folders");
                return StatusCode(500, new ApiResponseDto<List<UserFolderDto>>
                {
                    Success = false,
                    Message = "Internal server error while retrieving folders"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<UserFolderDto>>> GetFolder(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponseDto<UserFolderDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var folder = await _folderService.GetFolderByIdAsync(id, userId);

                if (folder == null)
                {
                    return NotFound(new ApiResponseDto<UserFolderDto>
                    {
                        Success = false,
                        Message = "Folder not found"
                    });
                }

                return Ok(new ApiResponseDto<UserFolderDto>
                {
                    Success = true,
                    Message = "Folder retrieved successfully",
                    Data = folder
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving folder: {Id}", id);
                return StatusCode(500, new ApiResponseDto<UserFolderDto>
                {
                    Success = false,
                    Message = "Internal server error while retrieving folder"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<UserFolderDto>>> CreateFolder([FromBody] CreateFolderDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<UserFolderDto>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponseDto<UserFolderDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var folder = await _folderService.CreateFolderAsync(userId, request);

                return CreatedAtAction(nameof(GetFolder), new { id = folder.Id }, new ApiResponseDto<UserFolderDto>
                {
                    Success = true,
                    Message = "Folder created successfully",
                    Data = folder
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating folder");
                return StatusCode(500, new ApiResponseDto<UserFolderDto>
                {
                    Success = false,
                    Message = "Internal server error while creating folder"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateFolder(int id, [FromBody] UpdateFolderDto request)
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

                var success = await _folderService.UpdateFolderAsync(id, userId, request);

                if (!success)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Folder not found"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Folder updated successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating folder: {Id}", id);
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Internal server error while updating folder"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteFolder(int id)
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

                var success = await _folderService.DeleteFolderAsync(id, userId);

                if (!success)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Folder not found"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Folder deleted successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting folder: {Id}", id);
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Internal server error while deleting folder"
                });
            }
        }

        [HttpPost("reorder")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ReorderFolders([FromBody] ReorderFoldersDto request)
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

                var success = await _folderService.ReorderFoldersAsync(userId, request.FolderIds);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = success,
                    Message = success ? "Folders reordered successfully" : "Failed to reorder folders",
                    Data = success
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering folders");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Internal server error while reordering folders"
                });
            }
        }
    }

    public class ReorderFoldersDto
    {
        public List<int> FolderIds { get; set; } = new();
    }
}
