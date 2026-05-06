using DeliveryService.Enums;
using Messaging.Contracts.Common.Models;

namespace DeliveryService.Entities
{
    public class ShipperAssignment : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ShipperId { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
        public string? RejectReason { get; set; }
    }
}
