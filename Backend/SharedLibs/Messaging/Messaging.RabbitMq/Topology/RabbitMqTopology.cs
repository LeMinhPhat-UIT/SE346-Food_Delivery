using Messaging.RabbitMq.Options;
using RabbitMQ.Client;

namespace Messaging.RabbitMq.Topology
{
    public static class RabbitMqTopology
    {
        public static async Task EnsureTopologyAsync(IChannel channel, RabbitMqOptions options)
        {
            foreach (var exchange in options.Exchanges)
            {
                await channel.ExchangeDeclareAsync(
                    exchange: exchange.Name,
                    type: exchange.Type,
                    durable: exchange.Durable,
                    autoDelete: false);

                foreach (var queue in exchange.Queues)
                {
                    var args = new Dictionary<string, object?>();

                    if (!string.IsNullOrWhiteSpace(queue.DeadLetterExchange))
                    {
                        args["x-dead-letter-exchange"] = queue.DeadLetterExchange;

                        await channel.ExchangeDeclareAsync(
                            exchange: queue.DeadLetterExchange,
                            type: ExchangeType.Fanout,
                            durable: true,
                            autoDelete: false);
                    }

                    await channel.QueueDeclareAsync(
                        queue: queue.Name,
                        durable: queue.Durable,
                        exclusive: false,
                        autoDelete: false,
                        arguments: args);

                    foreach (var routingKey in queue.RoutingKeys)
                    {
                        await channel.QueueBindAsync(
                            exchange: exchange.Name,
                            queue: queue.Name,
                            routingKey: routingKey);
                    }
                }
            }
        }
    }
}
