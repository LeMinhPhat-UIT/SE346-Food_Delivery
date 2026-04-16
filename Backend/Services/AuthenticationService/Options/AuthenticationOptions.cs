namespace AuthenticationService.Options
{
    public class AuthenticationOptions
    {
        public LockoutOptions LockoutSettings { get; set; } = new();
        public PasswordOptions PasswordSettings { get; set; } = new();
        public SignInOptions SignInSettings { get; set; } = new();
        public UserOptions UserSettings { get; set; } = new();
        public OtpOptions OtpSettings { get; set; } = new();
    }
}
