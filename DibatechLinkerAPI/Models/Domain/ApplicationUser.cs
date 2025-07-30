using Microsoft.AspNetCore.Identity;

namespace DibatechLinkerAPI.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public ReminderFrequency ReminderFrequency { get; set; } = ReminderFrequency.Weekly;
        public int? PreferredReminderDay { get; set; } // 0-6 for Sunday-Saturday
        public TimeOnly? PreferredReminderTime { get; set; }
        public bool IsEmailNotificationEnabled { get; set; } = true;
        public bool IsPushNotificationEnabled { get; set; } = false;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation properties
        public virtual ICollection<SavedLink> SavedLinks { get; set; } = new List<SavedLink>();
        public virtual ICollection<UserFolder> Folders { get; set; } = new List<UserFolder>();
    }
}
