namespace AuthenticationService.DTOs
{
    public class LogoutRequest
    {
        public string DeviceName { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
