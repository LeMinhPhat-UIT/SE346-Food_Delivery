namespace DeliveryService.Entities
{
    public class DeliveryFeeEstimate
    {
        public Guid? QuoteId { get; set; }
        public Guid? OrderId { get; set; }
        public decimal PickupLat { get; set; }
        public decimal PickupLng { get; set; }
        public decimal DeliveryLat { get; set; }
        public decimal DeliveryLng { get; set; }
        public decimal DistanceKm { get; set; }
        public decimal Subtotal { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        public decimal BaseFee { get; set; }
        public decimal DistanceFee { get; set; }
        public decimal SmallOrderSurcharge { get; set; }
        public decimal RushHourSurcharge { get; set; }
        public decimal RawFee { get; set; }
        public decimal DeliveryFee { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool IsSmallOrder { get; set; }
        public bool IsRushHour { get; set; }
        public bool IsWithinDeliveryRadius { get; set; }
        public decimal MaxDeliveryDistanceKm { get; set; }
        public List<DeliveryFeePolicyFeeBreakdown> PolicyBreakdowns { get; set; } = new List<DeliveryFeePolicyFeeBreakdown>();
    }
}
