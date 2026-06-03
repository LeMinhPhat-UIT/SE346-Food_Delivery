namespace DeliveryService.DTOs
{
    public class ActiveAssignmentOfferResponse
    {
        public bool HasActiveOffer { get; set; }
        public Guid? AssignmentId { get; set; }
        public Guid? OfferId { get; set; }
        public Guid? OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}
