namespace DeliveryService.DTOs
{
    public class ActiveAssignmentOfferResponse
    {
        public bool HasActiveOffer { get; set; }
        public Guid? AssignmentId { get; set; }
        public Guid? OfferId { get; set; }
        public Guid? OrderId { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
