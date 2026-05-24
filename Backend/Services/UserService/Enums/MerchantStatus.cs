using System.Text.Json.Serialization;

namespace UserService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MerchantStatus
    {
        Pending,
        Approved,
        Rejected,
        Suspended
    }
}
