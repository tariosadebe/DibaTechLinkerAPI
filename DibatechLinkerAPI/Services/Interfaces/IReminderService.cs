using DibatechLinkerAPI.Models.Domain;
using DibatechLinkerAPI.Models.DTOs;

namespace DibatechLinkerAPI.Services.Interfaces
{
    public interface IReminderService
    {
        Task<bool> SubscribeToRemindersAsync(string userId, ReminderSubscriptionDto request);
        Task<ReminderStatusDto?> GetReminderStatusAsync(string userId);
        Task<bool> UnsubscribeFromRemindersAsync(string userId);
        Task ProcessDailyRemindersAsync();
        Task ProcessWeeklyRemindersAsync();
        Task<bool> SendReminderEmailAsync(string userId);
    }
}
