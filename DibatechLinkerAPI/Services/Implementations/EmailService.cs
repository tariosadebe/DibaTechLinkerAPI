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
            _logger.LogInformation("Sending welcome email to {Email} for user {UserName}", toEmail, userName);
            
            if (_sendGridClient == null)
            {
                _logger.LogError("SendGrid client is null - API key not configured");
                return false;
            }

            try
            {
                var fromEmail = _configuration["SendGrid:FromEmail"];
                var fromName = _configuration["SendGrid:FromName"];
                
                var from = new EmailAddress(fromEmail, fromName);
                var to = new EmailAddress(toEmail);
                var subject = "Welcome to DibaTech Linker - Your Digital Link Manager";

                var htmlContent = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to DibaTech Linker</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f8f9fa;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff;'>
        
        <!-- Header with Cover Image -->
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); height: 200px; position: relative; text-align: center; color: white;'>
            <div style='position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); width: 100%;'>
                <h1 style='margin: 0; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>DibaTech Linker</h1>
                <p style='margin: 8px 0 0 0; font-size: 18px; opacity: 0.9;'>Professional Link Management</p>
            </div>
        </div>

        <!-- Welcome Content -->
        <div style='padding: 40px 30px;'>
            <h2 style='color: #2c3e50; font-size: 28px; margin: 0 0 20px 0; font-weight: 600;'>Welcome, {userName}</h2>
            
            <p style='color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;'>
                Thank you for joining DibaTech Linker. Your account has been successfully created and you can now start organizing your digital links with our powerful management platform.
            </p>

            <!-- Feature Highlights -->
            <div style='background-color: #f7fafc; border-radius: 8px; padding: 24px; margin: 30px 0;'>
                <h3 style='color: #2d3748; font-size: 20px; margin: 0 0 16px 0; font-weight: 600;'>What you can do with DibaTech Linker:</h3>
                
                <div style='margin-bottom: 16px;'>
                    <div style='display: inline-block; width: 8px; height: 8px; background-color: #667eea; border-radius: 50%; margin-right: 12px; vertical-align: middle;'></div>
                    <span style='color: #4a5568; font-size: 15px;'>Save and organize links from any website instantly</span>
                </div>
                
                <div style='margin-bottom: 16px;'>
                    <div style='display: inline-block; width: 8px; height: 8px; background-color: #667eea; border-radius: 50%; margin-right: 12px; vertical-align: middle;'></div>
                    <span style='color: #4a5568; font-size: 15px;'>Automatic content extraction and categorization</span>
                </div>
                
                <div style='margin-bottom: 16px;'>
                    <div style='display: inline-block; width: 8px; height: 8px; background-color: #667eea; border-radius: 50%; margin-right: 12px; vertical-align: middle;'></div>
                    <span style='color: #4a5568; font-size: 15px;'>Create custom folders for better organization</span>
                </div>
                
                <div style='margin-bottom: 0;'>
                    <div style='display: inline-block; width: 8px; height: 8px; background-color: #667eea; border-radius: 50%; margin-right: 12px; vertical-align: middle;'></div>
                    <span style='color: #4a5568; font-size: 15px;'>Set reminders to revisit important content</span>
                </div>
            </div>

            <!-- Call to Action -->
            <div style='text-align: center; margin: 32px 0;'>
                <a href='https://linker.dibatech.ng' style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; padding: 14px 32px; border-radius: 6px; font-weight: 600; font-size: 16px; transition: transform 0.2s;'>
                    Start Managing Your Links
                </a>
            </div>

            <p style='color: #718096; font-size: 14px; line-height: 1.5; margin: 24px 0 0 0;'>
                If you have any questions or need assistance, our support team is here to help. Simply reply to this email or contact us through our support portal.
            </p>
        </div>

        <!-- Footer -->
        <div style='background-color: #edf2f7; padding: 24px 30px; border-top: 1px solid #e2e8f0;'>
            <div style='text-align: center;'>
                <p style='color: #718096; font-size: 14px; margin: 0 0 8px 0;'>
                    <strong>DibaTech Linker</strong> - Professional Link Management Platform
                </p>
                <p style='color: #a0aec0; font-size: 12px; margin: 0;'>
                    This email was sent from a notification-only address. Please do not reply to this email.
                </p>
            </div>
        </div>
    </div>
</body>
</html>";

                var plainTextContent = $@"Welcome to DibaTech Linker, {userName}!

Thank you for joining DibaTech Linker. Your account has been successfully created and you can now start organizing your digital links with our powerful management platform.

What you can do with DibaTech Linker:
• Save and organize links from any website instantly
• Automatic content extraction and categorization  
• Create custom folders for better organization
• Set reminders to revisit important content

Get started: https://linker.dibatech.ng

If you have any questions or need assistance, our support team is here to help.

Best regards,
The DibaTech Team

---
DibaTech Linker - Professional Link Management Platform
This email was sent from a notification-only address. Please do not reply to this email.";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        
                var response = await _sendGridClient.SendEmailAsync(msg);
        
                var success = response.IsSuccessStatusCode;
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
                _logger.LogError(ex, "Exception occurred while sending welcome email to {Email}", toEmail);
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