using System.Text.Json.Serialization;

namespace DeliveryService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IncidentType
    {
        WrongOrder, MissingItem, Damaged, LateDelivery, RudeBehavior, Other
    }
}
