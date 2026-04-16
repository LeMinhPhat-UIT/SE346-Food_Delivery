namespace AuthenticationService.DTOs
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
    }
}
