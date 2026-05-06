namespace DeliveryService.DTOs
{
    public class NewAssignmentResponse
    {
        public Guid AssignmentId { get; set; }
        public Guid OrderId { get; set; }

        public string PickupAddress { get; set; } = null!;
        public string DeliveryAddress { get; set; } = null!;

        public decimal PickupLat { get; set; }
        public decimal PickupLng { get; set; }
        public decimal DeliveryLat { get; set; }
        public decimal DeliveryLng { get; set; }

        public decimal TotalFee { get; set; }
    }
}
