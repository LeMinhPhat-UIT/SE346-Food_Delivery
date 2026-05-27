namespace DeliveryService.DTOs
{
    public class RejectAssignmentRequest
    {
        public Guid? OfferId { get; set; }
        public string? Reason { get; set; }
    }
}
