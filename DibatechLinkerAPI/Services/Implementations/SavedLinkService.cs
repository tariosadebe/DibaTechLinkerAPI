using Microsoft.EntityFrameworkCore;
using DibatechLinkerAPI.Data;
using DibatechLinkerAPI.Models.Domain;
using DibatechLinkerAPI.Models.DTOs;
using DibatechLinkerAPI.Services.Interfaces;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace DibatechLinkerAPI.Services.Implementations
{
    public class SavedLinkService : ISavedLinkService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SavedLinkService> _logger;

        public SavedLinkService(ApplicationDbContext context, ILogger<SavedLinkService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SavedLink> SaveLinkAsync(int parsedLinkId, string? userId, string? sessionId, SaveLinkRequestDto request)
        {
            var parsedLink = await _context.ParsedLinks.FindAsync(parsedLinkId);
            if (parsedLink == null)
                throw new ArgumentException("ParsedLink not found");

            // Check if link is already saved by this user/session
            var existingLink = await _context.SavedLinks
                .FirstOrDefaultAsync(sl => sl.ParsedLinkId == parsedLinkId && 
                    ((userId != null && sl.UserId == userId) || 
                     (sessionId != null && sl.SessionId == sessionId)));

            if (existingLink != null)
                return existingLink;

            var savedLink = new SavedLink
            {
                ParsedLinkId = parsedLinkId,
                UserId = userId,
                SessionId = sessionId,
                CustomNote = request.CustomNote,
                CustomTitle = request.CustomTitle,
                FolderId = request.FolderId,
                Tags = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null,
                SavedAt = DateTime.UtcNow,
                Status = LinkStatus.Unread
            };

            _context.SavedLinks.Add(savedLink);
            await _context.SaveChangesAsync();

            return savedLink;
        }

        public async Task<PaginatedResponseDto<SavedLinkDto>> GetUserSavedLinksAsync(
            string? userId, string? sessionId, int pageNumber, int pageSize, 
            string? category = null, string? status = null, int? folderId = null)
        {
            var query = _context.SavedLinks
                .Include(sl => sl.ParsedLink)
                .Include(sl => sl.Folder)
                .Where(sl => (userId != null && sl.UserId == userId) || 
                            (sessionId != null && sl.SessionId == sessionId));

            // Apply filters
            if (!string.IsNullOrEmpty(category) && Enum.TryParse<LinkCategory>(category, true, out var categoryEnum))
            {
                query = query.Where(sl => sl.ParsedLink.Category == categoryEnum);
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<LinkStatus>(status, true, out var statusEnum))
            {
                query = query.Where(sl => sl.Status == statusEnum);
            }

            if (folderId.HasValue)
            {
                query = query.Where(sl => sl.FolderId == folderId.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await query
                .OrderByDescending(sl => sl.SavedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(sl => new SavedLinkDto
                {
                    Id = sl.Id,
                    ParsedLink = new ParsedLinkDto
                    {
                        Id = sl.ParsedLink.Id,
                        OriginalUrl = sl.ParsedLink.OriginalUrl,
                        Title = sl.ParsedLink.Title,
                        Description = sl.ParsedLink.Description,
                        ImageUrl = sl.ParsedLink.ImageUrl,
                        Author = sl.ParsedLink.Author,
                        SiteName = sl.ParsedLink.SiteName,
                        Domain = sl.ParsedLink.Domain,
                        ContentType = sl.ParsedLink.ContentType.ToString(),
                        Category = sl.ParsedLink.Category.ToString(),
                        ParsedAt = sl.ParsedLink.ParsedAt,
                        IsValidUrl = sl.ParsedLink.IsValidUrl,
                        ErrorMessage = sl.ParsedLink.ErrorMessage
                    },
                    CustomNote = sl.CustomNote,
                    CustomTitle = sl.CustomTitle,
                    Status = sl.Status.ToString(),
                    SavedAt = sl.SavedAt,
                    ReadAt = sl.ReadAt,
                    Folder = sl.Folder != null ? new UserFolderDto
                    {
                        Id = sl.Folder.Id,
                        Name = sl.Folder.Name,
                        Description = sl.Folder.Description,
                        Color = sl.Folder.Color,
                        SortOrder = sl.Folder.SortOrder,
                        CreatedAt = sl.Folder.CreatedAt
                    } : null,
                    Tags = !string.IsNullOrEmpty(sl.Tags) ? JsonSerializer.Deserialize<List<string>>(sl.Tags, (JsonSerializerOptions?)null) : null,
                    ShareToken = sl.ShareToken,
                    IsPublicShare = sl.IsPublicShare
                })
                .ToListAsync();

            return new PaginatedResponseDto<SavedLinkDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };
        }

        public async Task<SavedLinkDto?> GetSavedLinkByIdAsync(int id, string? userId, string? sessionId)
        {
            var savedLink = await _context.SavedLinks
                .Include(sl => sl.ParsedLink)
                .Include(sl => sl.Folder)
                .FirstOrDefaultAsync(sl => sl.Id == id && 
                    ((userId != null && sl.UserId == userId) || 
                     (sessionId != null && sl.SessionId == sessionId)));

            if (savedLink == null)
                return null;

            return new SavedLinkDto
            {
                Id = savedLink.Id,
                ParsedLink = new ParsedLinkDto
                {
                    Id = savedLink.ParsedLink.Id,
                    OriginalUrl = savedLink.ParsedLink.OriginalUrl,
                    Title = savedLink.ParsedLink.Title,
                    Description = savedLink.ParsedLink.Description,
                    ImageUrl = savedLink.ParsedLink.ImageUrl,
                    Author = savedLink.ParsedLink.Author,
                    SiteName = savedLink.ParsedLink.SiteName,
                    Domain = savedLink.ParsedLink.Domain,
                    ContentType = savedLink.ParsedLink.ContentType.ToString(),
                    Category = savedLink.ParsedLink.Category.ToString(),
                    ParsedAt = savedLink.ParsedLink.ParsedAt,
                    IsValidUrl = savedLink.ParsedLink.IsValidUrl,
                    ErrorMessage = savedLink.ParsedLink.ErrorMessage
                },
                CustomNote = savedLink.CustomNote,
                CustomTitle = savedLink.CustomTitle,
                Status = savedLink.Status.ToString(),
                SavedAt = savedLink.SavedAt,
                ReadAt = savedLink.ReadAt,
                Folder = savedLink.Folder != null ? new UserFolderDto
                {
                    Id = savedLink.Folder.Id,
                    Name = savedLink.Folder.Name,
                    Description = savedLink.Folder.Description,
                    Color = savedLink.Folder.Color,
                    SortOrder = savedLink.Folder.SortOrder,
                    CreatedAt = savedLink.Folder.CreatedAt
                } : null,
                Tags = !string.IsNullOrEmpty(savedLink.Tags) ? JsonSerializer.Deserialize<List<string>>(savedLink.Tags, (JsonSerializerOptions?)null) : null,
                ShareToken = savedLink.ShareToken,
                IsPublicShare = savedLink.IsPublicShare
            };
        }

        public async Task<SavedLinkDto?> GetPublicSharedLinkAsync(string shareToken)
        {
            var savedLink = await _context.SavedLinks
                .Include(sl => sl.ParsedLink)
                .Include(sl => sl.Folder)
                .FirstOrDefaultAsync(sl => sl.ShareToken == shareToken && sl.IsPublicShare &&
                    (sl.ShareTokenExpiryAt == null || sl.ShareTokenExpiryAt > DateTime.UtcNow));

            if (savedLink == null)
                return null;

            return new SavedLinkDto
            {
                Id = savedLink.Id,
                ParsedLink = new ParsedLinkDto
                {
                    Id = savedLink.ParsedLink.Id,
                    OriginalUrl = savedLink.ParsedLink.OriginalUrl,
                    Title = savedLink.ParsedLink.Title,
                    Description = savedLink.ParsedLink.Description,
                    ImageUrl = savedLink.ParsedLink.ImageUrl,
                    Author = savedLink.ParsedLink.Author,
                    SiteName = savedLink.ParsedLink.SiteName,
                    Domain = savedLink.ParsedLink.Domain,
                    ContentType = savedLink.ParsedLink.ContentType.ToString(),
                    Category = savedLink.ParsedLink.Category.ToString(),
                    ParsedAt = savedLink.ParsedLink.ParsedAt,
                    IsValidUrl = savedLink.ParsedLink.IsValidUrl
                },
                CustomNote = savedLink.CustomNote,
                CustomTitle = savedLink.CustomTitle,
                SavedAt = savedLink.SavedAt,
                Tags = !string.IsNullOrEmpty(savedLink.Tags) ? JsonSerializer.Deserialize<List<string>>(savedLink.Tags, (JsonSerializerOptions?)null) : null
            };
        }

        public async Task<bool> UpdateSavedLinkAsync(int id, string? userId, string? sessionId, UpdateSavedLinkDto request)
        {
            var savedLink = await _context.SavedLinks
                .FirstOrDefaultAsync(sl => sl.Id == id && 
                    ((userId != null && sl.UserId == userId) || 
                     (sessionId != null && sl.SessionId == sessionId)));

            if (savedLink == null)
                return false;

            if (request.CustomNote != null)
                savedLink.CustomNote = request.CustomNote;

            if (request.CustomTitle != null)
                savedLink.CustomTitle = request.CustomTitle;

            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<LinkStatus>(request.Status, true, out var statusEnum))
            {
                savedLink.Status = statusEnum;
                if (statusEnum == LinkStatus.Read && savedLink.ReadAt == null)
                    savedLink.ReadAt = DateTime.UtcNow;
            }

            if (request.FolderId.HasValue)
                savedLink.FolderId = request.FolderId.Value;

            if (request.Tags != null)
                savedLink.Tags = JsonSerializer.Serialize(request.Tags);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSavedLinkAsync(int id, string? userId, string? sessionId)
        {
            var savedLink = await _context.SavedLinks
                .FirstOrDefaultAsync(sl => sl.Id == id && 
                    ((userId != null && sl.UserId == userId) || 
                     (sessionId != null && sl.SessionId == sessionId)));

            if (savedLink == null)
                return false;

            _context.SavedLinks.Remove(savedLink);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateShareTokenAsync(int savedLinkId, string? userId, string? sessionId, DateTime? expiryDate = null)
        {
            var savedLink = await _context.SavedLinks
                .FirstOrDefaultAsync(sl => sl.Id == savedLinkId && 
                    ((userId != null && sl.UserId == userId) || 
                     (sessionId != null && sl.SessionId == sessionId)));

            if (savedLink == null)
                throw new ArgumentException("SavedLink not found");

            var shareToken = GenerateRandomToken();
            savedLink.ShareToken = shareToken;
            savedLink.IsPublicShare = true;
            savedLink.ShareTokenExpiryAt = expiryDate;

            await _context.SaveChangesAsync();
            return shareToken;
        }

        public async Task<bool> MarkAsReadAsync(int id, string? userId, string? sessionId)
        {
            var savedLink = await _context.SavedLinks
                .FirstOrDefaultAsync(sl => sl.Id == id && 
                    ((userId != null && sl.UserId == userId) || 
                     (sessionId != null && sl.SessionId == sessionId)));

            if (savedLink == null)
                return false;

            savedLink.Status = LinkStatus.Read;
            savedLink.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleFavouriteAsync(int id, string? userId, string? sessionId)
        {
            var savedLink = await _context.SavedLinks
                .FirstOrDefaultAsync(sl => sl.Id == id && 
                    ((userId != null && sl.UserId == userId) || 
                     (sessionId != null && sl.SessionId == sessionId)));

            if (savedLink == null)
                return false;

            savedLink.Status = savedLink.Status == LinkStatus.Favourite ? LinkStatus.Read : LinkStatus.Favourite;

            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateRandomToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}
