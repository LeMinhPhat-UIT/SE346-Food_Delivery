namespace DeliveryService.DTOs
{
    public class DeliveryFeePolicyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BaseFee { get; set; }
        public decimal? MinFee { get; set; }
        public decimal? MaxFee { get; set; }
        public decimal? SmallOrderThreshold { get; set; }
        public decimal SmallOrderSurcharge { get; set; }
        public decimal RushHourSurcharge { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<DeliveryFeeDistanceTierResponse> DistanceTiers { get; set; } = new List<DeliveryFeeDistanceTierResponse>();
    }
}
