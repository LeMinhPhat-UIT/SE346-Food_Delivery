using System.Text.Json.Serialization;

namespace DeliveryService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ShipperWorkStatus
    {
        Offline,
        ActiveIdle,
        PendingAssignment,
        Delivering
    }
}
