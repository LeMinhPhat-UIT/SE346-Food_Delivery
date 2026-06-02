namespace AuthenticationService.DTOs
{
    public class VerifyResetOtpResponse
    {
        public string Message { get; set; } = null!;
        public string ResetToken { get; set; } = null!;
    }
}
