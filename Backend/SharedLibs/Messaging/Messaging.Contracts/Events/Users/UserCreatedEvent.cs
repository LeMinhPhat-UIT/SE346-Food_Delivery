using Messaging.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts.Events
{
    public class UserCreatedEvent : EventBase
    {
        public override string RoutingKey => "user.created";

        public Guid UserId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = null!;
        public string Phone { get; init; } = string.Empty;
    }
}
