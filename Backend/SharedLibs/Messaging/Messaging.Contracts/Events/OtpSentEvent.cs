using Messaging.Contracts.Models;

namespace Messaging.Contracts.Events
{
    public class OtpSentEvent : EventBase
    {
        public string Email { get; set; } = null!;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public override string RoutingKey => "otp.sent";
    }
}
