namespace DeliveryService.DTOs
{
    public class EstimateDeliveryFeeRequest
    {
        public decimal? PickupLat { get; set; }
        public decimal? PickupLng { get; set; }
        public decimal? DeliveryLat { get; set; }
        public decimal? DeliveryLng { get; set; }
    }
}
