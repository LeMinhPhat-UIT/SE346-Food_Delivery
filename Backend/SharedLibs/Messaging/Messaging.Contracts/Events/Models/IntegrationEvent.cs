namespace Messaging.Contracts.Models
{
    public class IntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public string? CorrelationId { get; set; }
    }
}
