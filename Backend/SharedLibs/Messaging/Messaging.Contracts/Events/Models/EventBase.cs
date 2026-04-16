namespace Messaging.Contracts.Models
{
    public abstract class EventBase : IntegrationEvent, IRoutableEvent
    {
        public abstract string RoutingKey { get; }
    }
}
