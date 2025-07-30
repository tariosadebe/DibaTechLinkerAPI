using System.ComponentModel.DataAnnotations;

namespace DibatechLinkerAPI.Models.Domain
{
    public class ReminderJob
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public ReminderFrequency Frequency { get; set; }
        public DateTime NextRunAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int UnreadLinksCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
