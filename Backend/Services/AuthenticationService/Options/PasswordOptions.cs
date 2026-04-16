namespace AuthenticationService.Options
{
    public class PasswordOptions
    {
        public bool RequireDigit { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public int RequiredLength { get; set; }
    }
}
