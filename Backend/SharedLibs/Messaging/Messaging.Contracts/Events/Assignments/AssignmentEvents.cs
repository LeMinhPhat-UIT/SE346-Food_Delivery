using Messaging.Contracts.Models;

namespace Messaging.Contracts.Events
{
    public class AssignmentOfferedEvent : EventBase
    {
        public override string RoutingKey => "assignment.offered";

        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Guid MerchantId { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public LocationPayload PickupLocation { get; set; } = new();
        public LocationPayload DropoffLocation { get; set; } = new();
        public IReadOnlyList<AssignmentOfferPayload> Offers { get; set; } = Array.Empty<AssignmentOfferPayload>();
    }

    public class AssignmentAcceptedEvent : EventBase
    {
        public override string RoutingKey => "assignment.accepted";

        public Guid AssignmentId { get; set; }
        public Guid OfferId { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Guid MerchantId { get; set; }
        public Guid AcceptedByShipperId { get; set; }
        public IReadOnlyList<Guid> CancelledOfferIds { get; set; } = Array.Empty<Guid>();
        public IReadOnlyList<Guid> CancelledShipperIds { get; set; } = Array.Empty<Guid>();
    }

    public class AssignmentExpiredEvent : EventBase
    {
        public override string RoutingKey => "assignment.expired";

        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public IReadOnlyList<AssignmentExpiredOfferPayload> Offers { get; set; } = Array.Empty<AssignmentExpiredOfferPayload>();
    }

    public class AssignmentRejectedEvent : EventBase
    {
        public override string RoutingKey => "assignment.rejected";

        public Guid AssignmentId { get; set; }
        public Guid OfferId { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid ShipperId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime RejectedAt { get; set; }
    }

    public class AssignmentOfferPayload
    {
        public Guid AssignmentId { get; set; }
        public Guid OfferId { get; set; }
        public Guid ShipperId { get; set; }
        public decimal EstimatedDistanceToMerchantKm { get; set; }
        public decimal EstimatedDeliveryDistanceKm { get; set; }
        public decimal EstimatedFee { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class AssignmentExpiredOfferPayload
    {
        public Guid AssignmentId { get; set; }
        public Guid OfferId { get; set; }
        public Guid ShipperId { get; set; }
        public DateTime ExpiredAt { get; set; }
    }

    public class LocationPayload
    {
        public string Address { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
