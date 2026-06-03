namespace UserService.DTOs.User
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = null!;

        public string AvatarFileKey { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Status { get; set; } = null!;

        public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    }
}
