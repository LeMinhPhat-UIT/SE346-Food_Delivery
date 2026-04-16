using Messaging.Abstractions.Dispatching;
using Messaging.Abstractions.Registry;
using Messaging.RabbitMq.Connection;
using Messaging.RabbitMq.Constants;
using Messaging.RabbitMq.Options;
using Messaging.RabbitMq.Topology;
using NotificationService.Consuming;
using Messaging.RabbitMq.Helpers;
using Microsoft.Extensions.Options;

namespace NotificationService.HostedService
{
    public class EventConsumerHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventConsumerHostedService> _logger;

        public EventConsumerHostedService(
            IServiceProvider serviceProvider,
            ILogger<EventConsumerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Event Consumer Hosted Service starting...");

            try
            {
                using var scope = _serviceProvider.CreateScope();

                var rabbitOptions = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                var connectionManager = scope.ServiceProvider.GetRequiredService<ConnectionManager>();
                var eventDispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                var eventTypeRegistry = scope.ServiceProvider.GetRequiredService<IEventTypeRegistry>();

                var connection = await connectionManager.GetConnectionAsync();
                var channel = await connection.CreateChannelAsync();
                
                await RabbitMqTopology.EnsureTopologyAsync(channel, rabbitOptions);

                var consumerLogger = loggerFactory.CreateLogger<GenericEventConsumer>();
                var consumer = new GenericEventConsumer(channel, eventDispatcher, consumerLogger, eventTypeRegistry);

                var queues = rabbitOptions
                                .Exchanges
                                .SelectMany(e => e.Queues)
                                .Select(q => q.Name)
                                .Distinct();

                var tasks = queues.Select(async queue =>
                {
                    var channel = await connection.CreateChannelAsync();

                    var consumer = new GenericEventConsumer(
                        channel,
                        eventDispatcher,
                        loggerFactory.CreateLogger<GenericEventConsumer>(),
                        eventTypeRegistry);

                    await consumer.StartAsync(queue, stoppingToken);

                    _logger.LogInformation("Started consumer for queue: {Queue}", queue);

                    _logger.LogInformation(
                        "Event Consumer started, listening on queue: {Queue}. " +
                        "Events will be automatically dispatched to registered handlers.",
                        queue);
                });

                await Task.WhenAll(tasks);

                //var queueName = rabbitOptions.GetQueue(QueueNames.OtpRequested);
                //await consumer.StartAsync(queueName, stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Event Consumer Hosted Service stopping...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Event Consumer Hosted Service");
                throw;
            }
        }
    }
}
