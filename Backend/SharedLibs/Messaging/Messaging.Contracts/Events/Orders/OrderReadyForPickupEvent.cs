using Messaging.Contracts.Models;

namespace Messaging.Contracts.Events
{
    public class OrderReadyForPickupEvent : EventBase
    {
        public override string RoutingKey => "order.ready_for_pickup";

        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;

        public Guid MerchantId { get; set; }
        public string MerchantStoreName { get; set; } = string.Empty;
        public MerchantAddressBaseDto MerchantAddress { get; set; } = new();

        public Guid UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public UserAddressBaseDto DeliveryAddress { get; set; } = new();

        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal DistanceKm { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
