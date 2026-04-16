using Messaging.Abstractions.Dispatching;
using Microsoft.Extensions.DependencyInjection;
using Messaging.Contracts.Models;

namespace Messaging.RabbitMq.Dispatching
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _provider;

        public EventDispatcher(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task Dispatch<T>(T @event) where T : EventBase
        {
            var handlers = _provider.GetServices<IEventHandler<T>>();

            foreach (var handler in handlers)
            {
                await handler.Handle(@event);
            }
        }
    }
}
