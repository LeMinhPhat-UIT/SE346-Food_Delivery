namespace DeliveryService.DTOs
{
    public class AcceptAssignmentRequest
    {
        public Guid AssignmentId { get; set; }
        public Guid? OfferId { get; set; }
        public bool IsAccepted { get; set; }
        public string? RejectReason { get; set; }
    }
}
