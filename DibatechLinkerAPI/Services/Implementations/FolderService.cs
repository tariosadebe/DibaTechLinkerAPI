using Microsoft.EntityFrameworkCore;
using DibatechLinkerAPI.Data;
using DibatechLinkerAPI.Models.Domain;
using DibatechLinkerAPI.Models.DTOs;
using DibatechLinkerAPI.Services.Interfaces;

namespace DibatechLinkerAPI.Services.Implementations
{
    public class FolderService : IFolderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FolderService> _logger;

        public FolderService(ApplicationDbContext context, ILogger<FolderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserFolderDto> CreateFolderAsync(string userId, CreateFolderDto request)
        {
            var folder = new UserFolder
            {
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                Color = request.Color,
                SortOrder = await GetNextSortOrderAsync(userId),
                CreatedAt = DateTime.UtcNow
            };

            _context.UserFolders.Add(folder);
            await _context.SaveChangesAsync();

            return new UserFolderDto
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                Color = folder.Color,
                SortOrder = folder.SortOrder,
                CreatedAt = folder.CreatedAt,
                SavedLinksCount = 0
            };
        }

        public async Task<List<UserFolderDto>> GetUserFoldersAsync(string userId)
        {
            return await _context.UserFolders
                .Where(f => f.UserId == userId)
                .OrderBy(f => f.SortOrder)
                .Select(f => new UserFolderDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Color = f.Color,
                    SortOrder = f.SortOrder,
                    CreatedAt = f.CreatedAt,
                    SavedLinksCount = f.SavedLinks.Count
                })
                .ToListAsync();
        }

        public async Task<UserFolderDto?> GetFolderByIdAsync(int id, string userId)
        {
            var folder = await _context.UserFolders
                .Where(f => f.Id == id && f.UserId == userId)
                .Select(f => new UserFolderDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Color = f.Color,
                    SortOrder = f.SortOrder,
                    CreatedAt = f.CreatedAt,
                    SavedLinksCount = f.SavedLinks.Count
                })
                .FirstOrDefaultAsync();

            return folder;
        }

        public async Task<bool> UpdateFolderAsync(int id, string userId, UpdateFolderDto request)
        {
            var folder = await _context.UserFolders
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (folder == null)
                return false;

            if (request.Name != null)
                folder.Name = request.Name;

            if (request.Description != null)
                folder.Description = request.Description;

            if (request.Color != null)
                folder.Color = request.Color;

            if (request.SortOrder.HasValue)
                folder.SortOrder = request.SortOrder.Value;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFolderAsync(int id, string userId)
        {
            var folder = await _context.UserFolders
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (folder == null)
                return false;

            // Move all saved links from this folder to no folder
            var savedLinks = await _context.SavedLinks
                .Where(sl => sl.FolderId == id)
                .ToListAsync();

            foreach (var savedLink in savedLinks)
            {
                savedLink.FolderId = null;
            }

            _context.UserFolders.Remove(folder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderFoldersAsync(string userId, List<int> folderIds)
        {
            var folders = await _context.UserFolders
                .Where(f => f.UserId == userId && folderIds.Contains(f.Id))
                .ToListAsync();

            for (int i = 0; i < folderIds.Count; i++)
            {
                var folder = folders.FirstOrDefault(f => f.Id == folderIds[i]);
                if (folder != null)
                {
                    folder.SortOrder = i;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<int> GetNextSortOrderAsync(string userId)
        {
            var maxSortOrder = await _context.UserFolders
                .Where(f => f.UserId == userId)
                .MaxAsync(f => (int?)f.SortOrder) ?? -1;

            return maxSortOrder + 1;
        }
    }
}
