using Messaging.Contracts.Models;

namespace Messaging.Contracts.Events
{
    public class OtpVerifiedEvent : EventBase
    {
        public override string RoutingKey => "otp.verified";

        public Guid UserId { get; init; }
    }
}
