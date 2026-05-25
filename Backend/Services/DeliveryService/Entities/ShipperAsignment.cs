using DeliveryService.Enums;
using Messaging.Contracts.Common.Models;

namespace DeliveryService.Entities
{
    public class ShipperAssignment : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid MerchantId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid ShipperId { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal DistanceKm { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? RejectReason { get; set; }
        public string? PickupProofFileKey { get; set; }
        public string? DeliveryProofFileKey { get; set; }
    }
}
