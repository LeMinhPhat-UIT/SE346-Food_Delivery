using DeliveryService.Enums;

namespace DeliveryService.DTOs
{
    public class ShipperStatusResponse
    {
        public Guid ShipperId { get; set; }
        public ShipperWorkStatus Status { get; set; }

        public Guid? CurrentOrderId { get; set; }
        public bool IsBusy => Status == ShipperWorkStatus.PendingAssignment || Status == ShipperWorkStatus.Delivering;

        public decimal CurrentLat { get; set; }
        public decimal CurrentLng { get; set; }
        public DateTime? LastSeenAt { get; set; }
    }
}
