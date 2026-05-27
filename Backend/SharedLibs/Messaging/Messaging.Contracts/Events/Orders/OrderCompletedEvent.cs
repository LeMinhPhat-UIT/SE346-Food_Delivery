using Messaging.Contracts.Models;

namespace Messaging.Contracts.Events
{
    public class OrderCompletedEvent : EventBase
    {
        public override string RoutingKey => "order.completed";

        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;

        public Guid MerchantId { get; set; }
        public string MerchantStoreName { get; set; } = string.Empty;
        public MerchantAddressBaseDto MerchantAddress { get; set; } = new MerchantAddressBaseDto();

        public Guid UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public UserAddressBaseDto DeliveryAddress { get; set; } = new UserAddressBaseDto();

        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal DistanceKm { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; 
        public string? Note { get; set; }
    }

    public class MerchantAddressBaseDto
    {
        public string AddressLine { get; set; } = string.Empty;
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }

    public class UserAddressBaseDto
    {
        public string AddressLine { get; set; } = string.Empty;
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }
}
