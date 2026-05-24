using System.Text.Json.Serialization;

namespace UserService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ShipperStatus
    {
        Pending,
        Approved,
        Rejected,
        Suspended
    }
}
