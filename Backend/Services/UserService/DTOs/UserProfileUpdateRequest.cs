using System.Text.Json.Serialization;

namespace UserService.DTOs
{
    public class UserProfileUpdateRequest
    {
        [JsonPropertyName("fullname")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("avatarUrl")]
        public string AvatarUrl { get; set; } = null!;
    }
}
