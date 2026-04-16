using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Abstractions.Registry
{
    public interface IEventTypeRegistry
    {
        Type? Get(string eventType);
    }
}
