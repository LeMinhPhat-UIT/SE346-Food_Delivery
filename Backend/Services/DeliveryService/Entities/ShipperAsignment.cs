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
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public decimal PickupLatitude { get; set; }
        public decimal PickupLongitude { get; set; }
        public string DropoffAddress { get; set; } = string.Empty;
        public decimal DropoffLatitude { get; set; }
        public decimal DropoffLongitude { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal DistanceKm { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Created;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? OfferExpiresAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? RejectReason { get; set; }
        public string? CancelledReason { get; set; }
        public string? PickupProofFileKey { get; set; }
        public string? DeliveryProofFileKey { get; set; }
    }
}
