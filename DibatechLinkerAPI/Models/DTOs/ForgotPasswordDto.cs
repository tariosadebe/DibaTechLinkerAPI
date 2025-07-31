using System.ComponentModel.DataAnnotations;

namespace DibatechLinkerAPI.Models.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        public string Email { get; set; } = string.Empty;
    }
}