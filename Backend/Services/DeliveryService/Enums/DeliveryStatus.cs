using System.Text.Json.Serialization;

namespace DeliveryService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeliveryStatus
    {
        Pending, Assigned, PickingUp, PickedUp, Delivering, Delivered, Failed
    }
}
