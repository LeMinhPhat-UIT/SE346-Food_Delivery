namespace DeliveryService.DTOs
{
    public class EstimateDeliveryFeeRequest
    {
        public Guid? OrderId { get; set; }
        public decimal? PickupLat { get; set; }
        public decimal? PickupLng { get; set; }
        public decimal? DeliveryLat { get; set; }
        public decimal? DeliveryLng { get; set; }
        public decimal? Subtotal { get; set; }
        public bool? IsRushHour { get; set; }
    }
}
