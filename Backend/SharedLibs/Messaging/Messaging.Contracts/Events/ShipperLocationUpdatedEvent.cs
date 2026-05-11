using Messaging.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts.Events
{
    public class ShipperLocationUpdatedEvent : EventBase
    {
        public Guid OrderId { get; set; }
        public Guid ShipperId { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public override string RoutingKey => "shipper.location.updated";
    }
}
