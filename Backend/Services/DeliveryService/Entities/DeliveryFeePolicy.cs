using Messaging.Contracts.Common.Models;

namespace DeliveryService.Entities
{
    public class DeliveryFeePolicy : BaseAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal BaseFee { get; set; }
        public decimal? MinFee { get; set; }
        public decimal? MaxFee { get; set; }
        public decimal? SmallOrderThreshold { get; set; }
        public decimal SmallOrderSurcharge { get; set; }
        public decimal RushHourSurcharge { get; set; }
        public bool IsActive { get; set; }
        public ICollection<DeliveryFeeDistanceTier> DistanceTiers { get; set; } = new List<DeliveryFeeDistanceTier>();
        public ICollection<DeliveryFeeQuoteDetail> QuoteDetails { get; set; } = new List<DeliveryFeeQuoteDetail>();
    }
}
