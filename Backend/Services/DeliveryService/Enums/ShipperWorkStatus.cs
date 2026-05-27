using System.Text.Json.Serialization;

namespace DeliveryService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ShipperWorkStatus
    {
        Offline,
        ActiveIdle,
        Offering,
        Busy,

        // Legacy values kept so old rows and clients can still deserialize.
        PendingAssignment,
        Delivering
    }
}
