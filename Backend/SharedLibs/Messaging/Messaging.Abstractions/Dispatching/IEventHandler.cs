using Messaging.Contracts.Models;

namespace Messaging.Abstractions.Dispatching
{
    public interface IEventHandler<T> where T : EventBase
    {
        Task Handle(T @event);
    }
}
