namespace AuthenticationService.DTOs
{
    public class SendOtpResponse
    {
        public string Message { get; set; } = null!;
        public int? ExpiresInSeconds { get; set; }
    }
}
