namespace AuthenticationService.DTOs
{
    public class RevokeTokenRequest
    {
        public string DeviceName { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
