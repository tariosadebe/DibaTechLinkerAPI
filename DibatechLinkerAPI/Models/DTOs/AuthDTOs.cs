using System.ComponentModel.DataAnnotations;

namespace DibatechLinkerAPI.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserProfileDto User { get; set; } = null!;
    }

    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string ReminderFrequency { get; set; } = string.Empty;
        public int? PreferredReminderDay { get; set; }
        public string? PreferredReminderTime { get; set; }
        public bool IsEmailNotificationEnabled { get; set; }
        public bool IsPushNotificationEnabled { get; set; }
    }

    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ReminderFrequency { get; set; }
        public int? PreferredReminderDay { get; set; }
        public string? PreferredReminderTime { get; set; }
        public bool? IsEmailNotificationEnabled { get; set; }
        public bool? IsPushNotificationEnabled { get; set; }
    }

    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
