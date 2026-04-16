namespace AuthenticationService.DTOs
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        //public string PhoneNumber { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
