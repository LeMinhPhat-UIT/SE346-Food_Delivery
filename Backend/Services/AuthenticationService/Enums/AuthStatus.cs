using System.Text.Json.Serialization;

namespace AuthenticationService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AuthStatus
    {
        Active,
        Locked,
        PendingVerification
    }
}
