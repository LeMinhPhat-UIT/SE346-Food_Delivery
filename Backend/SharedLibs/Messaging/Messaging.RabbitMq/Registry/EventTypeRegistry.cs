using Messaging.Abstractions.Registry;
using Messaging.Contracts.Models;

namespace Messaging.RabbitMq.Registry
{
    public class EventTypeRegistry : IEventTypeRegistry
    {
        private readonly Dictionary<string, Type> _map;
        private readonly Dictionary<string, Type> _routingKeyMap;

        public EventTypeRegistry()
        {
            _map = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => typeof(EventBase).IsAssignableFrom(t) && !t.IsAbstract)
                .ToDictionary(t => t.Name, t => t);

            _routingKeyMap = _map.Values
                .Select(type => new { Type = type, RoutingKey = TryGetRoutingKey(type) })
                .Where(item => !string.IsNullOrWhiteSpace(item.RoutingKey))
                .ToDictionary(item => item.RoutingKey!, item => item.Type);
        }

        public Type? Get(string eventType)
        {
            _map.TryGetValue(eventType, out var type);
            return type;
        }

        public Type? GetByRoutingKey(string routingKey)
        {
            _routingKeyMap.TryGetValue(routingKey, out var type);
            return type;
        }

        private static string? TryGetRoutingKey(Type type)
        {
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                return null;
            }

            return Activator.CreateInstance(type) is EventBase @event
                ? @event.RoutingKey
                : null;
        }
    }
}
