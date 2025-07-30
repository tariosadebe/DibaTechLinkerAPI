using DibatechLinkerAPI.Models.Domain;
using DibatechLinkerAPI.Models.DTOs;

namespace DibatechLinkerAPI.Services.Interfaces
{
    public interface IFolderService
    {
        Task<UserFolderDto> CreateFolderAsync(string userId, CreateFolderDto request);
        Task<List<UserFolderDto>> GetUserFoldersAsync(string userId);
        Task<UserFolderDto?> GetFolderByIdAsync(int id, string userId);
        Task<bool> UpdateFolderAsync(int id, string userId, UpdateFolderDto request);
        Task<bool> DeleteFolderAsync(int id, string userId);
        Task<bool> ReorderFoldersAsync(string userId, List<int> folderIds);
    }
}
