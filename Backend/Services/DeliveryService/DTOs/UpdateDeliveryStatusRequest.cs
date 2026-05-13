using DeliveryService.Enums;

namespace DeliveryService.DTOs
{
    public class UpdateDeliveryStatusRequest
    {
        public DeliveryStatus Status { get; set; }
        public string Note { get; set; } = null!;
        public string? ProofFileKey { get; set; }
    }
}
