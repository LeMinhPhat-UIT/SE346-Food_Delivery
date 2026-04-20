using System.Text.Json.Serialization;

namespace UserService.DTOs
{
    public class UserProfileUpdateRequest
    {
        public string FullName { get; set; } = null!;

        public string AvatarUrl { get; set; } = null!;
    }
}
