using Messaging.Abstractions.Dispatching;
using Messaging.Abstractions.Registry;
using Messaging.Contracts.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Messaging.RabbitMq.Consuming
{
    public class GenericEventConsumer
    {
        private readonly IChannel _channel;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ILogger<GenericEventConsumer> _logger;
        private readonly IEventTypeRegistry _eventTypeRegistry;

        public GenericEventConsumer(
            IChannel channel,
            IEventDispatcher eventDispatcher,
            ILogger<GenericEventConsumer> logger,
            IEventTypeRegistry eventTypeRegistry)
        {
            _channel = channel;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
            _eventTypeRegistry = eventTypeRegistry;
        }

        public async Task StartAsync(string queueName, CancellationToken cancellationToken = default)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var routingKey = ea.RoutingKey;
                var correlationId = ea.BasicProperties.CorrelationId ?? string.Empty;
                
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var (eventType, eventData, envelopeCorrelationId) = ResolveEvent(body, routingKey);

                    if (eventType == null)
                    {
                        _logger.LogWarning("Unknown event type for routing key: {RoutingKey}", routingKey);
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
                        return;
                    }

                    var @event = eventData.Deserialize(eventType) as EventBase;

                    if (@event == null)
                    {
                        _logger.LogError("Failed to deserialize event data to {EventType}", eventType.Name);
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
                        return;
                    }

                    if (string.IsNullOrEmpty(@event.CorrelationId))
                    {
                        @event.CorrelationId = envelopeCorrelationId ?? correlationId;
                    }

                    await DispatchEventAsync(@event, eventType);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error processing message with RoutingKey: {RoutingKey}, CorrelationId: {CorrelationId}",
                        routingKey,
                        correlationId);

                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Generic event consumer started on queue: {Queue}",
                queueName);
        }

        private (Type? EventType, JsonElement EventData, string? CorrelationId) ResolveEvent(string body, string routingKey)
        {
            EventEnvelope<JsonElement>? envelope = null;

            try
            {
                envelope = JsonSerializer.Deserialize<EventEnvelope<JsonElement>>(body);
            }
            catch (JsonException)
            {
                // Some non-.NET services publish the event body directly.
            }

            if (envelope != null &&
                !string.IsNullOrWhiteSpace(envelope.EventType) &&
                envelope.Data.ValueKind != JsonValueKind.Undefined)
            {
                return (_eventTypeRegistry.Get(envelope.EventType), envelope.Data, envelope.CorrelationId);
            }

            var rawEvent = JsonSerializer.Deserialize<JsonElement>(body);
            return (_eventTypeRegistry.GetByRoutingKey(routingKey), rawEvent, null);
        }

        private async Task DispatchEventAsync(EventBase @event, Type eventType)
        {
            var dispatchMethod = typeof(IEventDispatcher)
                .GetMethod(nameof(IEventDispatcher.Dispatch))
                ?.MakeGenericMethod(eventType);

            if (dispatchMethod == null)
            {
                throw new InvalidOperationException($"Could not find Dispatch method for type {eventType.Name}");
            }

            var task = dispatchMethod.Invoke(_eventDispatcher, new object[] { @event }) as Task;
            if (task != null)
            {
                await task;
            }
            //await _eventDispatcher.Dispatch(@event);
        }
    }
}
