using Messaging.Contracts.Common.Models;

namespace DeliveryService.Entities
{
    public class DeliveryFeeDistanceTier : BaseEntity
    {
        public Guid PolicyId { get; set; }
        public decimal FromKm { get; set; }
        public decimal? ToKm { get; set; }
        public decimal FeePerKm { get; set; }
        public DeliveryFeePolicy? Policy { get; set; }
    }
}
