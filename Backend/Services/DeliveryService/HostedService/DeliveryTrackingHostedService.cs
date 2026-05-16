using DeliveryService.Entities;
using DeliveryService.Repositories.Interfaces;
using Messaging.Contracts.Events;
using Messaging.Contracts.Models;
using Messaging.RabbitMq.Connection;
using Messaging.RabbitMq.Options;
using Messaging.RabbitMq.Topology;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DeliveryService.HostedService
{
    public class DeliveryTrackingHostedService : BackgroundService
    {
        private const string TrackingQueueName = "delivery-tracking-queue";
        private const string TrackingRoutingKey = "shipper.location.updated";
        private readonly ILogger<DeliveryTrackingHostedService> _logger;
        private readonly ConnectionManager _connectionManager;
        private readonly IOptions<RabbitMqOptions> _rabbitMqOptions;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly SemaphoreSlim _pendingLock = new(1, 1);
        private readonly List<PendingTrackingEvent> _pendingEvents = new();

        private IConnection? _connection;
        private IChannel? _channel;

        public DeliveryTrackingHostedService(
            ILogger<DeliveryTrackingHostedService> logger,
            ConnectionManager connectionManager,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _rabbitMqOptions = rabbitMqOptions;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Delivery tracking worker starting");

            try
            {
                _connection = await _connectionManager.GetConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                var rabbitOptions = _rabbitMqOptions.Value;
                var trackingQueue = rabbitOptions.Exchanges
                    .SelectMany(exchange => exchange.Queues)
                    .FirstOrDefault(queue => queue.Name == TrackingQueueName || queue.RoutingKeys.Contains(TrackingRoutingKey))
                    ?? throw new InvalidOperationException("Tracking queue is not configured under RabbitMq:Exchanges");

                await RabbitMqTopology.EnsureTopologyAsync(_channel, rabbitOptions);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += HandleLocationEventAsync;

                await _channel.BasicConsumeAsync(
                    queue: trackingQueue.Name,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(trackingQueue.FlushIntervalSeconds));

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    while (await FlushPendingEventsAsync(stoppingToken))
                    { }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Delivery tracking worker stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delivery tracking worker failed");
                throw;
            }
            finally
            {
                if (_channel != null)
                {
                    while (await FlushPendingEventsAsync(CancellationToken.None))
                    { }
                }
            }
        }

        private async Task HandleLocationEventAsync(object? sender, BasicDeliverEventArgs ea)
        {
            if (_channel is null)
                return;

            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var envelope = JsonSerializer.Deserialize<EventEnvelope<JsonElement>>(body);

                if (envelope is null)
                {
                    _logger.LogWarning("Discarding invalid tracking envelope");
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                var locationEvent = envelope.Data.Deserialize<ShipperLocationUpdatedEvent>();

                if (locationEvent is null)
                {
                    _logger.LogWarning("Discarding invalid tracking event payload");
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                bool shouldFlush;
                var batchSize = _rabbitMqOptions.Value.Exchanges
                    .SelectMany(exchange => exchange.Queues)
                    .First(queue => queue.Name == TrackingQueueName || queue.RoutingKeys.Contains(TrackingRoutingKey))
                    .BatchSize;

                await _pendingLock.WaitAsync();
                try
                {
                    _pendingEvents.Add(new PendingTrackingEvent(ea.DeliveryTag, locationEvent));
                    shouldFlush = _pendingEvents.Count >= batchSize;
                }
                finally
                {
                    _pendingLock.Release();
                }

                if (shouldFlush)
                    await FlushPendingEventsAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to buffer tracking update");
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }
        }

        private async Task<bool> FlushPendingEventsAsync(CancellationToken cancellationToken)
        {
            if (_channel is null)
            {
                return false;
            }

            List<PendingTrackingEvent> batch;

            await _pendingLock.WaitAsync(cancellationToken);
            try
            {
                if (_pendingEvents.Count == 0)
                {
                    return false;
                }

                batch = _pendingEvents.Take(_rabbitMqOptions.Value.Exchanges
                    .SelectMany(exchange => exchange.Queues)
                    .First(queue => queue.Name == TrackingQueueName || queue.RoutingKeys.Contains(TrackingRoutingKey))
                    .BatchSize).ToList();
                _pendingEvents.RemoveRange(0, batch.Count);
            }
            finally
            {
                _pendingLock.Release();
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var deliveryRepository = scope.ServiceProvider.GetRequiredService<IDeliveryRepository>();

                var entries = batch.Select(item => new ShipperLocationHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = item.Event.OrderId,
                    ShipperId = item.Event.ShipperId,
                    Latitude = item.Event.Latitude,
                    Longitude = item.Event.Longitude,
                    RecordedAt = item.Event.CreatedAt,
                    CorrelationId = item.Event.CorrelationId
                }).ToList();

                await deliveryRepository.AddShipperLocationHistoriesAsync(entries, cancellationToken);

                foreach (var item in batch)
                {
                    await _channel.BasicAckAsync(item.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                }

                _logger.LogInformation("Persisted {Count} tracking updates", batch.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist tracking batch; requeueing messages");

                foreach (var item in batch)
                {
                    await _channel.BasicNackAsync(item.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
                }

                return false;
            }
        }

        private sealed record PendingTrackingEvent(ulong DeliveryTag, ShipperLocationUpdatedEvent Event);
    }
}
