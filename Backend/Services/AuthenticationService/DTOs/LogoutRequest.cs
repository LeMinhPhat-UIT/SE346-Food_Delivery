namespace AuthenticationService.DTOs
{
    public class LogoutRequest
    {
        public string? DeviceName { get; set; }
        public string RefreshToken { get; set; } = null!;
    }
}
