using StackExchange.Redis;

namespace DeliveryService.Options
{
    public class DeliveryOption
    {
        public double DeliveryRadius { get; set; }
        public double FindingShipperRadius { get; set; }
        public GeoUnit GeoUnit { get; set; }
        public decimal BaseDeliveryFee { get; set; } = 10000m;
        public decimal FeePerKm { get; set; } = 5000m;
        public decimal MinimumDeliveryFee { get; set; } = 10000m;
        public string Currency { get; set; } = "VND";
    }
}
