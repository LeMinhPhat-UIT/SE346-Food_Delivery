using StackExchange.Redis;

namespace DeliveryService.Options
{
    public class DeliveryOption
    {
        public double DeliveryRadius { get; set; }
        public double FindingShipperRadius { get; set; }
        public GeoUnit GeoUnit { get; set; }
    }
}
