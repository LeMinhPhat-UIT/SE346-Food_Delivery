namespace Messaging.Contracts.Models
{
    public class EventEnvelope<T>
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; } = typeof(T).Name;
        public DateTime CreatedAt { get; set; }
        public int Version { get; set; }
        public string? CorrelationId { get; set; }

        public required T Data { get; set; }
    }
}
