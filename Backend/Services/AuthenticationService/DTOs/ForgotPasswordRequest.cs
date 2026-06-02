using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
