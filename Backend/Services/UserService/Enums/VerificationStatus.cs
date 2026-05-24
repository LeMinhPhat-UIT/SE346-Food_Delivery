using System.Text.Json.Serialization;

namespace UserService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VerificationStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
