namespace AuthenticationService.DTOs
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = null!;
        public string? DeviceName { get; set; }
    }
}
