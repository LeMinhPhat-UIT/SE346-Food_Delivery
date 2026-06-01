namespace DeliveryService.DTOs
{
    public class DeliveryFeePolicyRequest
    {
        public string? Name { get; set; }
        public decimal BaseFee { get; set; }
        public decimal? MinFee { get; set; }
        public decimal? MaxFee { get; set; }
        public decimal? SmallOrderThreshold { get; set; }
        public decimal SmallOrderSurcharge { get; set; }
        public decimal RushHourSurcharge { get; set; }
        public bool IsActive { get; set; }
        public List<DeliveryFeeDistanceTierRequest> DistanceTiers { get; set; } = new List<DeliveryFeeDistanceTierRequest>();
    }
}
