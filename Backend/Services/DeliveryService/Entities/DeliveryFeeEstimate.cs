namespace DeliveryService.Entities
{
    public class DeliveryFeeEstimate
    {
        public decimal PickupLat { get; set; }
        public decimal PickupLng { get; set; }
        public decimal DeliveryLat { get; set; }
        public decimal DeliveryLng { get; set; }
        public decimal DistanceKm { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        public decimal BaseFee { get; set; }
        public decimal DistanceFee { get; set; }
        public decimal DeliveryFee { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool IsWithinDeliveryRadius { get; set; }
        public decimal MaxDeliveryDistanceKm { get; set; }
    }
}
