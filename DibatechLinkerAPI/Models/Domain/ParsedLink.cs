using System.ComponentModel.DataAnnotations;

namespace DibatechLinkerAPI.Models.Domain
{
    public class ParsedLink
    {
        public int Id { get; set; }
        
        [Required]
        [Url]
        public string OriginalUrl { get; set; } = string.Empty;
        
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Author { get; set; }
        public string? SiteName { get; set; }
        public string? Domain { get; set; }
        public ContentType ContentType { get; set; } = ContentType.Unknown;
        public LinkCategory Category { get; set; } = LinkCategory.Uncategorized;
        public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
        public bool IsValidUrl { get; set; } = true;
        public string? ErrorMessage { get; set; }
        
        // Navigation properties
        public virtual ICollection<SavedLink> SavedLinks { get; set; } = new List<SavedLink>();
    }
}
