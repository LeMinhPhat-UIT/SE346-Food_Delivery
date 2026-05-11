using Messaging.Contracts.Common.Models;

namespace DeliveryService.Entities
{
    public class ShipperLocationHistory : BaseAuditableEntity
    {
        public Guid OrderId { get; set; }
        public Guid ShipperId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime RecordedAt { get; set; }
        public string? CorrelationId { get; set; }
    }
}