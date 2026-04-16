using Messaging.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts.Events
{
    public class LockedOutEvent : EventBase
    {
        public override string RoutingKey => "user.locked-out";

        public Guid UserId { get; init; }
        public string Email { get; init; } = null!;
        public string Message { get; init; } = null!;
        public DateTime LockoutEndDate { get; init; }
    }
}
