using Messaging.Contracts.Models;

namespace Messaging.Contracts.Events
{
    public class OtpSendRequestedEvent : EventBase
    {
        public override string RoutingKey => "otp.send-requested";

        public Guid UserId { get; init; }
        public string Email { get; init; }
        public string Otp { get; set; } = null!;
        public string OtpType { get; init; } = "register";
        public DateTime ExpiresAt { get; init; }

        public OtpSendRequestedEvent(Guid userId, string email, string otp)
        {
            UserId = userId;
            Email = email;
            Otp = otp;
            ExpiresAt = CreatedAt.AddMinutes(2);
        }
    }
}
