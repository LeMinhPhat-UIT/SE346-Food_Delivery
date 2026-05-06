using Messaging.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts.Events
{
    public class ShipperFoundEvent : EventBase
    {
        public override string RoutingKey => "shipper.found";

        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public IEnumerable<Guid> ShipperIds { get; set; } = Array.Empty<Guid>();

        public double MerchantLng { get; set; }
        public double MerchantLat { get; set; }

        public double CustomerLng { get; set; }
        public double CustomerLat { get; set; }
    }
}
