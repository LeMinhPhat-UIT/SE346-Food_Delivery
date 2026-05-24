namespace AuthenticationService.DTOs
{
    public class RevokeTokenRequest
    {
        public string? DeviceName { get; set; }
        public string RefreshToken { get; set; } = null!;
    }
}
