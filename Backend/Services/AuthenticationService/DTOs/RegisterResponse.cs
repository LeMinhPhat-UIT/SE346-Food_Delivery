using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool RequiresOtpVerification { get; set; }
    }
}
