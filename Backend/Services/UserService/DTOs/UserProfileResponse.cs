using System.Text.Json.Serialization;

namespace UserService.DTOs
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }

        [JsonPropertyName("fullname")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("avatarUrl")]
        public string AvatarUrl { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }
}
