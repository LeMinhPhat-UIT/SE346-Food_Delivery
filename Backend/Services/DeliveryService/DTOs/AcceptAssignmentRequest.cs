namespace DeliveryService.DTOs
{
    public class AcceptAssignmentRequest
    {
        public Guid AssignmentId { get; set; }
        public bool IsAccepted { get; set; }
        public string? RejectReason { get; set; }
    }
}
