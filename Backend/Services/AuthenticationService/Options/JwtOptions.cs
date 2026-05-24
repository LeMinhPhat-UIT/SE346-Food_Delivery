namespace AuthenticationService.Options
{
    public class JwtOptions
    {
        public string Key { get; set; } = null!;
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int AccessTokenMinutes { get; set; } = 15;
        public int RefreshTokenMinutes { get; set; } = 7 * 24 * 60;
    }
}
