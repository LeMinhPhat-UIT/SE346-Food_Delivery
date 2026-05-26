using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AuthenticationService.DTOs
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [JsonIgnore]
        public string? DeviceId { get; set; }

        [JsonIgnore]
        public string? DeviceName { get; set; }
    }
}
