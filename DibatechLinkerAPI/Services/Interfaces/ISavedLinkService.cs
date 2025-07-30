using DibatechLinkerAPI.Models.Domain;
using DibatechLinkerAPI.Models.DTOs;

namespace DibatechLinkerAPI.Services.Interfaces
{
    public interface ISavedLinkService
    {
        Task<SavedLink> SaveLinkAsync(int parsedLinkId, string? userId, string? sessionId, SaveLinkRequestDto request);
        Task<PaginatedResponseDto<SavedLinkDto>> GetUserSavedLinksAsync(string? userId, string? sessionId, int pageNumber, int pageSize, string? category = null, string? status = null, int? folderId = null);
        Task<SavedLinkDto?> GetSavedLinkByIdAsync(int id, string? userId, string? sessionId);
        Task<SavedLinkDto?> GetPublicSharedLinkAsync(string shareToken);
        Task<bool> UpdateSavedLinkAsync(int id, string? userId, string? sessionId, UpdateSavedLinkDto request);
        Task<bool> DeleteSavedLinkAsync(int id, string? userId, string? sessionId);
        Task<string> GenerateShareTokenAsync(int savedLinkId, string? userId, string? sessionId, DateTime? expiryDate = null);
        Task<bool> MarkAsReadAsync(int id, string? userId, string? sessionId);
        Task<bool> ToggleFavouriteAsync(int id, string? userId, string? sessionId);
    }
}
