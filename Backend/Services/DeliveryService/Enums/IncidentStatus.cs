using System.Text.Json.Serialization;

namespace DeliveryService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IncidentStatus
    {
        Pending, Investigating, Resolved, Closed
    }
}
