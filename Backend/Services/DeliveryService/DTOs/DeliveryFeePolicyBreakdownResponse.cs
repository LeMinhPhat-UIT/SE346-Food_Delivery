namespace DeliveryService.DTOs
{
    public class DeliveryFeePolicyBreakdownResponse
    {
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
    }
}
