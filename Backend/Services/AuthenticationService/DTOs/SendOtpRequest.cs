using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs
{
    public class SendOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
