namespace NotificationService.Realtime
{
    public interface IRealtimeConnectionTracker
    {
        void Add(string groupName, string connectionId);
        void Remove(string connectionId);
        bool HasConnections(string groupName);
    }
}
