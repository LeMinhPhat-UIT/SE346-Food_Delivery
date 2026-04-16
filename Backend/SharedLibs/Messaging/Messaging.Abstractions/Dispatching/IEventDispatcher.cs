using Messaging.Contracts.Models;

namespace Messaging.Abstractions.Dispatching
{
    public interface IEventDispatcher
    {
        Task Dispatch<T>(T @event) where T : EventBase;
    }
}
