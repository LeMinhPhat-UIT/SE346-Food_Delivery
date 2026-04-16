using RabbitMQ.Client;

namespace Messaging.RabbitMq.Options
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";

        public List<ExchangeConfig> Exchanges { get; set; } = new();
    }

    public class ExchangeConfig
    {
        public string Name { get; set; } = default!;
        public string Type { get; set; } = ExchangeType.Topic;
        public bool Durable { get; set; } = true;

        public List<QueueConfig> Queues { get; set; } = new();
    }

    public class QueueConfig
    {
        public string Name { get; set; } = default!;
        public bool Durable { get; set; } = true;

        public string? DeadLetterExchange { get; set; }

        public List<string> RoutingKeys { get; set; } = new();
    }
}
