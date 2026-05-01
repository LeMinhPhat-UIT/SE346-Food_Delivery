using System.Text.Json.Serialization;

namespace UserService.DTOs.User
{
    public class UpdateUserProfileRequest
    {
        public string FullName { get; set; } = null!;

        public string AvatarUrl { get; set; } = null!;
    }
}
