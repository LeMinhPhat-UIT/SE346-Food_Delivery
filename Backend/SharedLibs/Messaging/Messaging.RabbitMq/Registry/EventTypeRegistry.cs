using Messaging.Abstractions.Registry;
using Messaging.Contracts.Models;

namespace Messaging.RabbitMq.Registry
{
    public class EventTypeRegistry : IEventTypeRegistry
    {
        private readonly Dictionary<string, Type> _map;

        public EventTypeRegistry()
        {
            _map = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => typeof(EventBase).IsAssignableFrom(t) && !t.IsAbstract)
                .ToDictionary(t => t.Name, t => t);
        }

        public Type? Get(string eventType)
        {
            _map.TryGetValue(eventType, out var type);
            return type;
        }
    }
}
