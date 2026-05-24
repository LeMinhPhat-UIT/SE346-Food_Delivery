using System.Text.Json.Serialization;

namespace UserService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserStatus
    {
        Active,
        Inactive,
        Banned,
        PendingVerification
    }
}
