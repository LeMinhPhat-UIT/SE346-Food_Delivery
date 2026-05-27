using Messaging.Contracts.Models;

namespace Messaging.Contracts.Events
{
    public class DeliveryDeliveredEvent : EventBase
    {
        public override string RoutingKey => "delivery.delivered";

        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Guid ShipperId { get; set; }
        public Guid MerchantId { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal DistanceKm { get; set; }
        public DateTime DeliveryAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ProofFileKey { get; set; }
        public string? Note { get; set; }
    }
}
