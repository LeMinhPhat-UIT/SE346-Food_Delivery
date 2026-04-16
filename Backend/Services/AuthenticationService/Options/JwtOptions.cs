namespace AuthenticationService.Options
{
    public class JwtOptions
    {
        public string Key { get; set; } = null!;
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int AccessTokenMinutes { get; set; }
        public int RefreshTokenMinutes { get; set; }
    }
}
