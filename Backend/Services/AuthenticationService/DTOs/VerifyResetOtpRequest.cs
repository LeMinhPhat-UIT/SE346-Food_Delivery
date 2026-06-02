using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs
{
    public class VerifyResetOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Otp { get; set; } = null!;
    }
}
