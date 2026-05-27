using System.Collections.Concurrent;

namespace NotificationService.Realtime
{
    public class RealtimeConnectionTracker : IRealtimeConnectionTracker
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _groups = new();
        private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _connectionGroups = new();

        public void Add(string groupName, string connectionId)
        {
            var connections = _groups.GetOrAdd(groupName, _ => new ConcurrentDictionary<string, byte>());
            connections[connectionId] = 0;

            var groups = _connectionGroups.GetOrAdd(connectionId, _ => new ConcurrentBag<string>());
            groups.Add(groupName);
        }

        public void Remove(string connectionId)
        {
            if (!_connectionGroups.TryRemove(connectionId, out var groups))
                return;

            foreach (var group in groups.Distinct())
            {
                if (_groups.TryGetValue(group, out var connections))
                {
                    connections.TryRemove(connectionId, out _);
                    if (connections.IsEmpty)
                        _groups.TryRemove(group, out _);
                }
            }
        }

        public bool HasConnections(string groupName)
        {
            return _groups.TryGetValue(groupName, out var connections) && !connections.IsEmpty;
        }
    }
}
