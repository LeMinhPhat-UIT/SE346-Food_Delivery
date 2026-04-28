using System.Text.Json.Serialization;

namespace DeliveryService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AssignmentStatus
    {
        Pending, Accepted, Rejected, Timeout, Cancelled
    }
}
