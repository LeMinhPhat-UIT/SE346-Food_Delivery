namespace Messaging.Contracts.Models
{
    public interface IRoutableEvent
    {
        string RoutingKey { get; }
    }
}
