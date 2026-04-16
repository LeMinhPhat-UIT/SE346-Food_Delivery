namespace AuthenticationService.Options
{
    public class LockoutOptions
    {
        public bool IsLockoutOnFailure { get; set; }
        public int DefaultLockoutTimeSpanInMinutes { get; set; }
        public int DefaultMaxFailedAccessAttempt { get; set; }
    }
}
