using Microsoft.AspNetCore.Mvc;
using DibatechLinkerAPI.Services.Interfaces;

namespace DibatechLinkerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("test")]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                var success = await _emailService.SendTestEmailAsync(request.Email);
                
                if (success)
                {
                    return Ok(new { 
                        success = true, 
                        message = "✅ Test email sent successfully! Check your inbox." 
                    });
                }
                
                return BadRequest(new { 
                    success = false, 
                    message = "❌ Failed to send test email. Check your SendGrid configuration." 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = $"❌ Error: {ex.Message}" 
                });
            }
        }

        [HttpPost("welcome")]
        public async Task<IActionResult> SendWelcomeEmail([FromBody] WelcomeEmailRequest request)
        {
            try
            {
                var success = await _emailService.SendWelcomeEmailAsync(request.Email, request.Name);
                
                if (success)
                {
                    return Ok(new { 
                        success = true, 
                        message = "🎉 Welcome email sent successfully!" 
                    });
                }
                
                return BadRequest(new { 
                    success = false, 
                    message = "❌ Failed to send welcome email." 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = $"❌ Error: {ex.Message}" 
                });
            }
        }

        [HttpPost("reminder")]
        public async Task<IActionResult> SendReminderEmail([FromBody] ReminderEmailRequest request)
        {
            try
            {
                var success = await _emailService.SendReminderEmailAsync(request.Email, request.Name, request.LinkTitles);
                
                if (success)
                {
                    return Ok(new { 
                        success = true, 
                        message = "📬 Reminder email sent successfully!" 
                    });
                }
                
                return BadRequest(new { 
                    success = false, 
                    message = "❌ Failed to send reminder email." 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = $"❌ Error: {ex.Message}" 
                });
            }
        }
    }

    public class TestEmailRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class WelcomeEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class ReminderEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> LinkTitles { get; set; } = new();
    }
}