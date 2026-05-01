using System.Text.Json.Serialization;

namespace UserService.DTOs.User
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = null!;

        public string AvatarUrl { get; set; } = null!;

        public string Status { get; set; } = null!;
    }
}
