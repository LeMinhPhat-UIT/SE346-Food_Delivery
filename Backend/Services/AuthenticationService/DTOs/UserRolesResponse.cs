namespace AuthenticationService.DTOs
{
    public class UserRolesResponse
    {
        public Guid UserId { get; set; }
        public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    }
}
