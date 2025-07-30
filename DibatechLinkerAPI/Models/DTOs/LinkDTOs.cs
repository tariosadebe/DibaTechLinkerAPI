using System.ComponentModel.DataAnnotations;
using DibatechLinkerAPI.Models.Domain;

namespace DibatechLinkerAPI.Models.DTOs
{
    public class ParseLinkRequestDto
    {
        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;
    }

    public class ParsedLinkDto
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Author { get; set; }
        public string? SiteName { get; set; }
        public string? Domain { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime ParsedAt { get; set; }
        public bool IsValidUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class SaveLinkRequestDto
    {
        [Required]
        public int ParsedLinkId { get; set; }
        public string? CustomNote { get; set; }
        public string? CustomTitle { get; set; }
        public int? FolderId { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class SavedLinkDto
    {
        public int Id { get; set; }
        public ParsedLinkDto ParsedLink { get; set; } = null!;
        public string? CustomNote { get; set; }
        public string? CustomTitle { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public UserFolderDto? Folder { get; set; }
        public List<string>? Tags { get; set; }
        public string? ShareToken { get; set; }
        public bool IsPublicShare { get; set; }
    }

    public class UpdateSavedLinkDto
    {
        public string? CustomNote { get; set; }
        public string? CustomTitle { get; set; }
        public string? Status { get; set; }
        public int? FolderId { get; set; }
        public List<string>? Tags { get; set; }
    }
}
