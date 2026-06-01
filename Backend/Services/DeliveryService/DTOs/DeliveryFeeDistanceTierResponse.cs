namespace DeliveryService.DTOs
{
    public class DeliveryFeeDistanceTierResponse
    {
        public Guid Id { get; set; }
        public decimal FromKm { get; set; }
        public decimal? ToKm { get; set; }
        public decimal FeePerKm { get; set; }
    }
}
