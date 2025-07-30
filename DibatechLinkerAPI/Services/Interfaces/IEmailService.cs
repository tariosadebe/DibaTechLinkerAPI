namespace DibatechLinkerAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
        Task<bool> SendReminderEmailAsync(string toEmail, string userName, List<string> linkTitles);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken);
        Task<bool> SendTestEmailAsync(string toEmail);
    }
}