using Messaging.Contracts.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Messaging.RabbitMq.Publishing
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event) where T : EventBase;
    }

    public class EventPublisher : IEventPublisher
    {
        private readonly IChannel _channel;
        private readonly string _exchangeName;

        public EventPublisher(IChannel channel, string exchangeName)
        {
            _channel = channel;
            _exchangeName = exchangeName;
        }

        //public async Task PublishAsync<T>(string routingKey, T message, string? correlationId = null) where T : class
        //{
        //    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        //    var props = new BasicProperties
        //    {
        //        Persistent = true,
        //        CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
        //        ContentType = "application/json",
        //        Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        //    };

        //    await _channel.BasicPublishAsync(
        //        exchange: _exchangeName,
        //        routingKey: routingKey,
        //        mandatory: true,
        //        basicProperties: props,
        //        body: body);
        //}

        public async Task PublishAsync<T>(T @event) where T : EventBase
        {
            var envelope = new EventEnvelope<T>
            {
                EventId = @event.EventId,
                EventType = typeof(T).Name,
                CreatedAt = @event.CreatedAt,
                CorrelationId = @event.CorrelationId,
                Data = @event
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));

            var props = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                CorrelationId = @event.CorrelationId ?? Guid.NewGuid().ToString(),
                Type = typeof(T).Name
            };

            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: @event.RoutingKey,
                mandatory: true,
                basicProperties: props,
                body: body);
        }
    }
}
