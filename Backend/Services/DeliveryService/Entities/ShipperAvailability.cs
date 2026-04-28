using DeliveryService.Commons;

namespace DeliveryService.Entities
{
    public class ShipperAvailability : BaseAuditableEntity
    {
        public Guid ShipperId { get; set; } // Reference to User Service
        public bool IsAvailable { get; set; } = false;
        public decimal CurrentLat { get; set; }
        public decimal CurrentLng { get; set; }
        public DateTime? LastSeenAt { get; set; }
    }
}
