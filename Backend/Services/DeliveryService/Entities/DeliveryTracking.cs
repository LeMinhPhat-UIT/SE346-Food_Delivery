using DeliveryService.Enums;
using Messaging.Contracts.Common.Models;

namespace DeliveryService.Entities
{
    public class DeliveryTracking : BaseAuditableEntity
    {
        public Guid OrderId { get; set; } // Reference to Order Service
        public Guid? ShipperId { get; set; } // Reference to User Service
        public decimal PickupLat { get; set; }
        public decimal PickupLng { get; set; }
        public decimal DeliveryLat { get; set; }
        public decimal DeliveryLng { get; set; }
        public decimal DistanceKm { get; set; }
        public int EstimatedTime { get; set; } // Minutes
        public int? ActualTime { get; set; } // Minutes
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    }
}
