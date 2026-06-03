using System.Text.Json.Serialization;

namespace DeliveryService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AssignmentStatus
    {
        Created,
        Offering,
        Accepted,
        PickedUp,
        Delivering,
        Completed,
        Failed,
        Rejected,
        Expired,
        Cancelled,

        // Legacy values kept so old rows and clients can still deserialize.
        Pending,
        Timeout
    }
}
