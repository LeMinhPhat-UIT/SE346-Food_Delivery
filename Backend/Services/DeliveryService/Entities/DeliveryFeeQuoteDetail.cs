using Messaging.Contracts.Common.Models;

namespace DeliveryService.Entities
{
    public class DeliveryFeeQuoteDetail : BaseEntity
    {
        public Guid QuoteId { get; set; }
        public Guid PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public decimal BaseFee { get; set; }
        public decimal DistanceFee { get; set; }
        public decimal SmallOrderSurcharge { get; set; }
        public decimal RushHourSurcharge { get; set; }
        public decimal RawFee { get; set; }
        public decimal FinalFee { get; set; }
        public bool IsSmallOrder { get; set; }
        public bool IsRushHour { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DeliveryFeeQuote? Quote { get; set; }
        public DeliveryFeePolicy? Policy { get; set; }
    }
}
