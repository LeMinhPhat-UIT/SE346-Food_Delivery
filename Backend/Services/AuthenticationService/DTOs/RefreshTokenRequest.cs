using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AuthenticationService.DTOs
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = null!;

        [JsonIgnore]
        public string? DeviceId { get; set; }

        [JsonIgnore]
        public string? DeviceName { get; set; }
    }
}
