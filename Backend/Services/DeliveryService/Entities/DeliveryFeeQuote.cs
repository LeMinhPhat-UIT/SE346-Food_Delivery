using Messaging.Contracts.Common.Models;

namespace DeliveryService.Entities
{
    public class DeliveryFeeQuote : BaseEntity
    {
        public Guid? OrderId { get; set; }
        public decimal PickupLat { get; set; }
        public decimal PickupLng { get; set; }
        public decimal DropoffLat { get; set; }
        public decimal DropoffLng { get; set; }
        public decimal DistanceKm { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public string Currency { get; set; } = "VND";
        public bool IsRushHour { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<DeliveryFeeQuoteDetail> Details { get; set; } = new List<DeliveryFeeQuoteDetail>();
    }
}
