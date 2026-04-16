using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Messaging.RabbitMq.Consuming
{
    public abstract class BaseConsumer<T> where T : class
    {
        private readonly IChannel _channel;
        private readonly string _queue;
        private readonly ILogger _logger;

        protected BaseConsumer(IChannel channel, string queue, ILogger logger)
        {
            _channel = channel;
            _queue = queue;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<T>(body);
                    if (message != null)
                    {
                        await HandleMessageAsync(message, ea.BasicProperties.CorrelationId ?? string.Empty, cancellationToken);
                        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message from queue {Queue}", _queue);
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken);
                }
            };

            await _channel.BasicConsumeAsync(queue: _queue, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
            _logger.LogInformation("Started consuming from queue {Queue}", _queue);
        }

        protected abstract Task HandleMessageAsync(T message, string correlationId, CancellationToken cancellationToken);
    }
}
