using Microsoft.EntityFrameworkCore;
using DibatechLinkerAPI.Data;
using DibatechLinkerAPI.Models.Domain;
using DibatechLinkerAPI.Models.DTOs;
using DibatechLinkerAPI.Services.Interfaces;

namespace DibatechLinkerAPI.Services.Implementations
{
    public class ReminderService : IReminderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(ApplicationDbContext context, ILogger<ReminderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> SubscribeToRemindersAsync(string userId, ReminderSubscriptionDto request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                // Parse frequency
                if (!Enum.TryParse<ReminderFrequency>(request.Frequency, true, out var frequency))
                    return false;

                // Update user preferences
                user.ReminderFrequency = frequency;
                user.PreferredReminderDay = request.PreferredDay;
                
                TimeOnly? preferredTime = null;
                if (!string.IsNullOrEmpty(request.PreferredTime) && TimeOnly.TryParse(request.PreferredTime, out var time))
                {
                    user.PreferredReminderTime = time;
                    preferredTime = time;
                }

                if (request.IsEmailEnabled.HasValue)
                    user.IsEmailNotificationEnabled = request.IsEmailEnabled.Value;

                if (request.IsPushEnabled.HasValue)
                    user.IsPushNotificationEnabled = request.IsPushEnabled.Value;

                // Create or update reminder job
                var existingJob = await _context.ReminderJobs.FirstOrDefaultAsync(r => r.UserId == userId);
                
                if (existingJob == null)
                {
                    var reminderJob = new ReminderJob
                    {
                        UserId = userId,
                        Frequency = frequency,
                        NextRunAt = CalculateNextRunTime(frequency, request.PreferredDay, preferredTime),
                        IsActive = frequency != ReminderFrequency.None,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.ReminderJobs.Add(reminderJob);
                }
                else
                {
                    existingJob.Frequency = frequency;
                    existingJob.NextRunAt = CalculateNextRunTime(frequency, request.PreferredDay, preferredTime);
                    existingJob.IsActive = frequency != ReminderFrequency.None;
                    existingJob.LastModifiedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to reminders for user {UserId}", userId);
                return false;
            }
        }

        public async Task<ReminderStatusDto?> GetReminderStatusAsync(string userId)
        {
            try
            {
                var reminderJob = await _context.ReminderJobs
                    .FirstOrDefaultAsync(r => r.UserId == userId);

                if (reminderJob == null)
                    return null;

                // Get unread links count
                var unreadCount = await _context.SavedLinks
                    .Where(sl => sl.UserId == userId && sl.Status == LinkStatus.Unread)
                    .CountAsync();

                return new ReminderStatusDto
                {
                    Frequency = reminderJob.Frequency.ToString(),
                    NextRunAt = reminderJob.NextRunAt,
                    LastRunAt = reminderJob.LastRunAt,
                    IsActive = reminderJob.IsActive,
                    UnreadLinksCount = unreadCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reminder status for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UnsubscribeFromRemindersAsync(string userId)
        {
            try
            {
                var reminderJob = await _context.ReminderJobs
                    .FirstOrDefaultAsync(r => r.UserId == userId);

                if (reminderJob != null)
                {
                    reminderJob.IsActive = false;
                    reminderJob.LastModifiedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from reminders for user {UserId}", userId);
                return false;
            }
        }

        public async Task ProcessDailyRemindersAsync()
        {
            try
            {
                var dailyJobs = await _context.ReminderJobs
                    .Where(r => r.IsActive && r.Frequency == ReminderFrequency.Daily && r.NextRunAt <= DateTime.UtcNow)
                    .ToListAsync();

                foreach (var job in dailyJobs)
                {
                    await SendReminderEmailAsync(job.UserId);
                    
                    // Update next run time
                    job.LastRunAt = DateTime.UtcNow;
                    job.NextRunAt = DateTime.UtcNow.AddDays(1);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing daily reminders");
            }
        }

        public async Task ProcessWeeklyRemindersAsync()
        {
            try
            {
                var weeklyJobs = await _context.ReminderJobs
                    .Where(r => r.IsActive && r.Frequency == ReminderFrequency.Weekly && r.NextRunAt <= DateTime.UtcNow)
                    .ToListAsync();

                foreach (var job in weeklyJobs)
                {
                    await SendReminderEmailAsync(job.UserId);
                    
                    // Update next run time
                    job.LastRunAt = DateTime.UtcNow;
                    job.NextRunAt = DateTime.UtcNow.AddDays(7);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing weekly reminders");
            }
        }

        public async Task<bool> SendReminderEmailAsync(string userId)
        {
            try
            {
                // Get user details
                var user = await _context.Users.FindAsync(userId);
                if (user == null || !user.IsEmailNotificationEnabled)
                    return false;

                // Get unread links
                var unreadLinks = await _context.SavedLinks
                    .Include(sl => sl.ParsedLink)
                    .Where(sl => sl.UserId == userId && sl.Status == LinkStatus.Unread)
                    .OrderByDescending(sl => sl.SavedAt)
                    .Take(10)
                    .ToListAsync();

                if (!unreadLinks.Any())
                    return true; // No unread links, but not an error

                // TODO: Implement actual email sending logic here
                // For now, just log the reminder
                _logger.LogInformation("Reminder email would be sent to {Email} with {Count} unread links", 
                    user.Email, unreadLinks.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reminder email for user {UserId}", userId);
                return false;
            }
        }

        private DateTime CalculateNextRunTime(ReminderFrequency frequency, int? preferredDay, TimeOnly? preferredTime)
        {
            var baseTime = preferredTime ?? new TimeOnly(9, 0); // Default to 9 AM
            var now = DateTime.UtcNow;

            return frequency switch
            {
                ReminderFrequency.Daily => now.Date.AddDays(1).Add(baseTime.ToTimeSpan()),
                ReminderFrequency.Weekly => CalculateNextWeeklyRun(now, preferredDay ?? 1, baseTime), // Default to Monday
                _ => now.AddYears(1) // Effectively disabled
            };
        }

        private DateTime CalculateNextWeeklyRun(DateTime now, int preferredDay, TimeOnly preferredTime)
        {
            // preferredDay: 0 = Sunday, 1 = Monday, ..., 6 = Saturday
            var daysUntilNext = ((int)preferredDay - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilNext == 0) // Today is the preferred day
            {
                var todayAtPreferredTime = now.Date.Add(preferredTime.ToTimeSpan());
                if (now < todayAtPreferredTime)
                {
                    return todayAtPreferredTime;
                }
                else
                {
                    daysUntilNext = 7; // Next week
                }
            }

            return now.Date.AddDays(daysUntilNext).Add(preferredTime.ToTimeSpan());
        }
    }
}
