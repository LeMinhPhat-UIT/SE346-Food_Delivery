using DeliveryService.Enums;
using Messaging.Contracts.Common.Models;

namespace DeliveryService.Entities
{
    public class ShipperAvailability : BaseAuditableEntity
    {
        public Guid ShipperId { get; set; } // Reference to User Service

        public ShipperWorkStatus Status { get; set; } = ShipperWorkStatus.Offline;
        public Guid? CurrentOrderId { get; set; }

        public decimal CurrentLat { get; set; }
        public decimal CurrentLng { get; set; }
        public DateTime? LastSeenAt { get; set; }

        public bool IsEligibleForAssignment => Status == ShipperWorkStatus.ActiveIdle;
    }
}
