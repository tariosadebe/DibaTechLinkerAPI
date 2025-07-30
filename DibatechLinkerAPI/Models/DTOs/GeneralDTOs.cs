using System.ComponentModel.DataAnnotations;

namespace DibatechLinkerAPI.Models.DTOs
{
    public class UserFolderDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SavedLinksCount { get; set; }
    }

    public class CreateFolderDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        public string? Color { get; set; }
    }

    public class UpdateFolderDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public int? SortOrder { get; set; }
    }

    public class ReminderSubscriptionDto
    {
        [Required]
        public string Frequency { get; set; } = string.Empty; // Daily, Weekly, None
        
        public int? PreferredDay { get; set; } // 0-6 for Sunday-Saturday (for weekly)
        public string? PreferredTime { get; set; } // HH:mm format
        public bool? IsEmailEnabled { get; set; }
        public bool? IsPushEnabled { get; set; }
    }

    public class ReminderStatusDto
    {
        public string Frequency { get; set; } = string.Empty;
        public DateTime? NextRunAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        public bool IsActive { get; set; }
        public int UnreadLinksCount { get; set; }
    }

    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
    }

    public class PaginatedResponseDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
