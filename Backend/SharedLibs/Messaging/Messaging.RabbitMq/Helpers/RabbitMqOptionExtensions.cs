using Messaging.RabbitMq.Options;

namespace Messaging.RabbitMq.Helpers
{
    public static class RabbitMqOptionsExtensions
    {
        public static string GetQueue(this RabbitMqOptions options, string queueName)
        {
            return options.Exchanges
                .SelectMany(e => e.Queues)
                .FirstOrDefault(q => q.Name == queueName)?.Name
                ?? throw new InvalidOperationException($"Queue '{queueName}' not found");
        }
    }
}
