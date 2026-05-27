using Messaging.Contracts.Models;
using System.Text.Json.Serialization;

namespace Messaging.Contracts.Events
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeliveryMilestoneType
    {
        PickedUp,
        Delivered
    }

    public class DeliveryMilestoneEvent : EventBase
    {
        public override string RoutingKey => "delivery.milestone";

        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Guid ShipperId { get; set; }
        public DeliveryMilestoneType Milestone { get; set; }
        public string? ProofFileKey { get; set; }
        public string? Note { get; set; }
    }
}
