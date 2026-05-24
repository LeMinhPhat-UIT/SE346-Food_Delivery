using Messaging.Abstractions.Dispatching;
using Microsoft.Extensions.DependencyInjection;
using Messaging.Contracts.Models;

namespace Messaging.RabbitMq.Dispatching
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EventDispatcher(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task Dispatch<T>(T @event) where T : EventBase
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var handlers = scope.ServiceProvider.GetServices<IEventHandler<T>>();

            foreach (var handler in handlers)
            {
                await handler.Handle(@event);
            }
        }
    }
}
