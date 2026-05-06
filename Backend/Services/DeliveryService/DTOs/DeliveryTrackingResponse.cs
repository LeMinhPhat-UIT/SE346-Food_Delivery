using DeliveryService.Enums;

namespace DeliveryService.DTOs
{
    public class DeliveryTrackingResponse
    {
        public Guid OrderId { get; set; }
        public DeliveryStatus Status { get; set; }
        public decimal CurrentLat { get; set; }
        public decimal CurrentLng { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        public decimal DistanceKm { get; set; }
    }
}
