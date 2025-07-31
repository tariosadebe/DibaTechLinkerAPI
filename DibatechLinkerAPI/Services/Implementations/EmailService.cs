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

        private const string SendGridFromEmailKey = "SendGrid:FromEmail";
        private const string SendGridFromNameKey = "SendGrid:FromName";

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
                var fromEmail = _configuration[SendGridFromEmailKey];
                var fromName = _configuration[SendGridFromNameKey];
                
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
                var fromEmail = _configuration[SendGridFromEmailKey];
                var fromName = _configuration[SendGridFromNameKey];
                
                var from = new EmailAddress(fromEmail, fromName);
                var to = new EmailAddress(toEmail);
                var subject = "Link Reminder - DibaTech Linker";

                var linksHtml = string.Join("", linkTitles.Take(10).Select(title => 
                    $@"<div style='margin-bottom: 12px; padding: 12px; background-color: #f7fafc; border-left: 4px solid #667eea; border-radius: 4px;'>
                <span style='color: #4a5568; font-size: 15px;'>{title}</span>
               </div>"));

                var htmlContent = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Link Reminder - DibaTech Linker</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f8f9fa;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff;'>
        
        <!-- Header with Cover Image -->
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); height: 200px; position: relative; text-align: center; color: white;'>
            <div style='position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); width: 100%;'>
                <h1 style='margin: 0; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>DibaTech Linker</h1>
                <p style='margin: 8px 0 0 0; font-size: 18px; opacity: 0.9;'>Link Reminder Service</p>
            </div>
        </div>

        <!-- Reminder Content -->
        <div style='padding: 40px 30px;'>
            <h2 style='color: #2c3e50; font-size: 28px; margin: 0 0 20px 0; font-weight: 600;'>Hello, {userName}</h2>
            
            <p style='color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;'>
                You have <strong>{linkTitles.Count}</strong> saved links that are ready for your attention. Here are your recent saves waiting to be explored:
            </p>

            <!-- Links List -->
            <div style='background-color: #f7fafc; border-radius: 8px; padding: 24px; margin: 30px 0;'>
                <h3 style='color: #2d3748; font-size: 20px; margin: 0 0 20px 0; font-weight: 600;'>Your Saved Links:</h3>
                {linksHtml}
            </div>

            <!-- Call to Action -->
            <div style='text-align: center; margin: 32px 0;'>
                <a href='https://linker.dibatech.ng' style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; padding: 14px 32px; border-radius: 6px; font-weight: 600; font-size: 16px;'>
                    View All Your Links
                </a>
            </div>

            <p style='color: #718096; font-size: 14px; line-height: 1.5; margin: 24px 0 0 0;'>
                This reminder was sent based on your notification preferences. You can adjust your reminder settings in your account dashboard.
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

                var plainTextContent = $@"Link Reminder - DibaTech Linker

Hello {userName},

You have {linkTitles.Count} saved links ready for your attention:

{string.Join("\n", linkTitles.Take(10).Select((title, index) => $"{index + 1}. {title}"))}

View all your links: https://linker.dibatech.ng

This reminder was sent based on your notification preferences.

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
            _logger.LogInformation("Reminder email sent successfully to {Email}", toEmail);
        }
        else
        {
            _logger.LogWarning("Failed to send reminder email to {Email}. Status: {Status}", toEmail, response.StatusCode);
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
        var fromEmail = _configuration[SendGridFromEmailKey];
        var fromName = _configuration[SendGridFromNameKey];
        
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(toEmail);
        var subject = "Password Reset Request - DibaTech Linker";

        var resetUrl = $"https://linker.dibatech.ng/reset-password?token={resetToken}";
        
        var htmlContent = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Password Reset - DibaTech Linker</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f8f9fa;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff;'>
        
        <!-- Header with Cover Image -->
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); height: 200px; position: relative; text-align: center; color: white;'>
            <div style='position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); width: 100%;'>
                <h1 style='margin: 0; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>DibaTech Linker</h1>
                <p style='margin: 8px 0 0 0; font-size: 18px; opacity: 0.9;'>Account Security</p>
            </div>
        </div>

        <!-- Password Reset Content -->
        <div style='padding: 40px 30px;'>
            <h2 style='color: #2c3e50; font-size: 28px; margin: 0 0 20px 0; font-weight: 600;'>Password Reset Request</h2>
            
            <p style='color: #4a5568; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;'>
                We received a request to reset your password for your DibaTech Linker account. If you made this request, click the button below to create a new password.
            </p>

            <!-- Security Notice -->
            <div style='background-color: #fff5f5; border-left: 4px solid #f56565; padding: 16px; margin: 24px 0; border-radius: 4px;'>
                <p style='color: #c53030; font-size: 14px; margin: 0; font-weight: 600;'>Security Notice:</p>
                <p style='color: #c53030; font-size: 14px; margin: 8px 0 0 0;'>This reset link expires in 1 hour for your security. If you did not request this reset, please ignore this email.</p>
            </div>

            <!-- Call to Action -->
            <div style='text-align: center; margin: 32px 0;'>
                <a href='{resetUrl}' style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; padding: 14px 32px; border-radius: 6px; font-weight: 600; font-size: 16px;'>
                    Reset Your Password
                </a>
            </div>

            <p style='color: #718096; font-size: 14px; line-height: 1.5; margin: 24px 0 0 0;'>
                If the button above doesn't work, copy and paste this link into your browser:<br>
                <a href='{resetUrl}' style='color: #667eea; word-break: break-all;'>{resetUrl}</a>
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

                var plainTextContent = $@"Password Reset Request - DibaTech Linker

We received a request to reset your password for your DibaTech Linker account.

Reset your password using this link: {resetUrl}

Security Notice:
- This reset link expires in 1 hour for your security
- If you did not request this reset, please ignore this email

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
                    _logger.LogInformation("Password reset email sent successfully to {Email}", toEmail);
                }
                else
                {
                    _logger.LogWarning("Failed to send password reset email to {Email}. Status: {Status}", toEmail, response.StatusCode);
                }

                return success;
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
                var from = new EmailAddress(_configuration[SendGridFromEmailKey], _configuration[SendGridFromNameKey]);
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