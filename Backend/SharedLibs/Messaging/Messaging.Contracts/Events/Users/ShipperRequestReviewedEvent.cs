using Messaging.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts.Events
{
    public class ShipperRequestReviewedEvent : EventBase
    {
        public override string RoutingKey => "shipper.request.reviewed";

        public Guid RequestId { get; set; }
        public Guid UserId { get; set; }
        public Guid ReviewerId { get; set; }
        public bool IsApproved { get; set; }
        public string? RejectedReason { get; set; }
    }
}
