namespace DeliveryService.Entities
{
    public class DeliveryFeeEstimateInput
    {
        public decimal PickupLat { get; set; }
        public decimal PickupLng { get; set; }
        public decimal DeliveryLat { get; set; }
        public decimal DeliveryLng { get; set; }
    }
}
