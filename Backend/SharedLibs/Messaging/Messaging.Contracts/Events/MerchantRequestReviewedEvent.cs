using Messaging.Contracts.Models;

namespace Messaging.Contracts.Events
{
    public class MerchantRequestReviewedEvent : EventBase
    {
        public override string RoutingKey => "merchant.request.reviewed";

        public Guid RequestId { get; set; }
        public Guid UserId { get; set; }
        public Guid ReviewerId { get; set; }
        public bool IsApproved { get; set; }
        public string? RejectedReason { get; set; }
    }
}