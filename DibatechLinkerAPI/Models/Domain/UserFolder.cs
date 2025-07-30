using System.ComponentModel.DataAnnotations;

namespace DibatechLinkerAPI.Models.Domain
{
    public class UserFolder
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        public string? Color { get; set; } // Hex color code
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<SavedLink> SavedLinks { get; set; } = new List<SavedLink>();
    }
}
