namespace DeliveryService.DTOs
{
    public class DeliveryFeeDistanceTierRequest
    {
        public decimal FromKm { get; set; }
        public decimal? ToKm { get; set; }
        public decimal FeePerKm { get; set; }
    }
}
