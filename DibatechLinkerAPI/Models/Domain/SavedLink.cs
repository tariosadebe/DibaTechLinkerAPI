using System.ComponentModel.DataAnnotations;

namespace DibatechLinkerAPI.Models.Domain
{
    public class SavedLink
    {
        public int Id { get; set; }
        
        [Required]
        public int ParsedLinkId { get; set; }
        
        public string? UserId { get; set; } // Null for anonymous users
        public string? SessionId { get; set; } // For anonymous users
        public string? CustomNote { get; set; }
        public string? CustomTitle { get; set; }
        public LinkStatus Status { get; set; } = LinkStatus.Unread;
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public int? FolderId { get; set; }
        public string? Tags { get; set; } // JSON array or comma-separated
        public string? ShareToken { get; set; } // For public sharing
        public DateTime? ShareTokenExpiryAt { get; set; }
        public bool IsPublicShare { get; set; } = false;
        
        // Navigation properties
        public virtual ParsedLink ParsedLink { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; }
        public virtual UserFolder? Folder { get; set; }
    }
}
