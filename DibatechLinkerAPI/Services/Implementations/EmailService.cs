using SendGrid;
using SendGrid.Helpers.Mail;
using DibatechLinkerAPI.Services.Interfaces;

namespace DibatechLinkerAPI.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            var apiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_SENDGRID_API_KEY_HERE")
            {
                _logger.LogWarning("SendGrid API key not configured. Email functionality will be disabled.");
                _sendGridClient = null!;
            }
            else
            {
                _sendGridClient = new SendGridClient(apiKey);
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            if (_sendGridClient == null)
            {
                _logger.LogWarning("SendGrid not configured. Cannot send welcome email to {Email}", toEmail);
                return false;
            }

            try
            {
                var from = new EmailAddress(_configuration["SendGrid:FromEmail"], _configuration["SendGrid:FromName"]);
                var to = new EmailAddress(toEmail, userName);
                var subject = "Welcome to DibaTech Linker! 🎉";

                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #3498db; margin: 0;'>Welcome to DibaTech Linker! 🎉</h1>
                        </div>
                        
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px; color: white; text-align: center; margin-bottom: 30px;'>
                            <h2 style='margin: 0 0 15px 0;'>Hi {userName}!</h2>
                            <p style='margin: 0; font-size: 18px; line-height: 1.6;'>
                                Thank you for joining our community of smart link organizers! 
                                Let's help you save, organize, and never lose track of important content again.
                            </p>
                        </div>

                        <div style='margin-bottom: 30px;'>
                            <h3 style='color: #2c3e50;'>🚀 Here's what you can do:</h3>
                            <ul style='color: #555; line-height: 1.8;'>
                                <li><strong>Save Any Link:</strong> Just paste a URL and we'll extract all the important details</li>
                                <li><strong>Smart Organization:</strong> Create custom folders and use tags to organize your content</li>
                                <li><strong>Never Forget:</strong> Set up personalized reminders to revisit your saved content</li>
                                <li><strong>Share & Collaborate:</strong> Share your favorite links with secure sharing tokens</li>
                            </ul>
                        </div>

                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='https://localhost:7283' 
                               style='background: #3498db; color: white; padding: 15px 30px; 
                                      text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold; font-size: 16px;'>
                                🔗 Start Saving Links
                            </a>
                        </div>

                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                        <div style='text-align: center;'>
                            <small style='color: #95a5a6;'>
                                Powered by <strong>DibaTech.ng</strong><br>
                                Building digital solutions that work
                            </small>
                        </div>
                    </div>";

                var plainTextContent = $"Welcome to DibaTech Linker, {userName}! Start saving and organizing your links today.";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await _sendGridClient.SendEmailAsync(msg);

                var success = response.StatusCode == System.Net.HttpStatusCode.Accepted;
                if (success)
                {
                    _logger.LogInformation("Welcome email sent successfully to {Email}", toEmail);
                }
                else
                {
                    _logger.LogWarning("Failed to send welcome email to {Email}. Status: {Status}", toEmail, response.StatusCode);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendReminderEmailAsync(string toEmail, string userName, List<string> linkTitles)
        {
            if (_sendGridClient == null)
            {
                _logger.LogWarning("SendGrid not configured. Cannot send reminder email to {Email}", toEmail);
                return false;
            }

            try
            {
                var from = new EmailAddress(_configuration["SendGrid:FromEmail"], _configuration["SendGrid:FromName"]);
                var to = new EmailAddress(toEmail, userName);
                var subject = "Your Link Reminder - DibaTech Linker";

                var linksHtml = string.Join("", linkTitles.Take(10).Select(title => 
                    $"<li style='margin-bottom: 8px; padding: 8px; background: #f8f9fa; border-left: 3px solid #3498db;'>{title}</li>"));

                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #3498db; margin: 0;'>DibaTech Linker</h1>
                        </div>
                        <h2 style='color: #2c3e50;'>Hi {userName}! 👋</h2>
                        <p style='color: #555; font-size: 16px; line-height: 1.6;'>
                            You have <strong>{linkTitles.Count}</strong> saved links waiting for you to explore:
                        </p>
                        <ul style='list-style: none; padding: 0; margin: 20px 0;'>
                            {linksHtml}
                        </ul>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='https://localhost:7283' 
                               style='background: #3498db; color: white; padding: 12px 24px; 
                                      text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;'>
                                📖 Read Your Links
                            </a>
                        </div>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                        <div style='text-align: center;'>
                            <small style='color: #95a5a6;'>
                                Powered by <strong>DibaTech.ng</strong>
                            </small>
                        </div>
                    </div>";

                var plainTextContent = $"Hi {userName}! You have {linkTitles.Count} saved links: {string.Join(", ", linkTitles.Take(5))}";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await _sendGridClient.SendEmailAsync(msg);

                var success = response.StatusCode == System.Net.HttpStatusCode.Accepted;
                if (success)
                {
                    _logger.LogInformation("Reminder email sent successfully to {Email}", toEmail);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            if (_sendGridClient == null)
            {
                _logger.LogWarning("SendGrid not configured. Cannot send password reset email to {Email}", toEmail);
                return false;
            }

            try
            {
                var from = new EmailAddress(_configuration["SendGrid:FromEmail"], _configuration["SendGrid:FromName"]);
                var to = new EmailAddress(toEmail);
                var subject = "Reset Your Password - DibaTech Linker";

                var resetUrl = $"https://localhost:7283/reset-password?token={resetToken}";
                
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #3498db; margin: 0;'>DibaTech Linker</h1>
                        </div>
                        
                        <h2 style='color: #856404;'>🔒 Password Reset Request</h2>
                        <p style='color: #555; font-size: 16px; line-height: 1.6;'>
                            We received a request to reset your password. Click the button below to reset it:
                        </p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetUrl}' 
                               style='background: #e74c3c; color: white; padding: 15px 30px; 
                                      text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;'>
                                🔑 Reset Password
                            </a>
                        </div>

                        <p style='color: #666; font-size: 14px;'>
                            <strong>⏰ Important:</strong> This link expires in 1 hour for security reasons.
                        </p>

                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ecf0f1;'>
                        <div style='text-align: center;'>
                            <small style='color: #95a5a6;'>
                                Powered by <strong>DibaTech.ng</strong>
                            </small>
                        </div>
                    </div>";

                var plainTextContent = $"Reset your DibaTech Linker password using this link: {resetUrl} (expires in 1 hour)";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await _sendGridClient.SendEmailAsync(msg);

                return response.StatusCode == System.Net.HttpStatusCode.Accepted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendTestEmailAsync(string toEmail)
        {
            if (_sendGridClient == null)
            {
                _logger.LogWarning("SendGrid not configured. Cannot send test email to {Email}", toEmail);
                return false;
            }

            try
            {
                var from = new EmailAddress(_configuration["SendGrid:FromEmail"], _configuration["SendGrid:FromName"]);
                var to = new EmailAddress(toEmail);
                var subject = "Test Email - DibaTech Linker";

                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h1 style='color: #3498db;'>✅ Email Service Working!</h1>
                        <p>This is a test email from DibaTech Linker API.</p>
                        <p><strong>Sent at:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                        <p>If you received this email, the email service is configured correctly! 🎉</p>
                    </div>";

                var plainTextContent = $"Test email from DibaTech Linker API sent at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await _sendGridClient.SendEmailAsync(msg);

                var success = response.StatusCode == System.Net.HttpStatusCode.Accepted;
                _logger.LogInformation("Test email sent to {Email}. Success: {Success}", toEmail, success);
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email to {Email}", toEmail);
                return false;
            }
        }
    }
}