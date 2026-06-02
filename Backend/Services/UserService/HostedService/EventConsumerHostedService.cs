using Messaging.Abstractions.Dispatching;
using Messaging.Abstractions.Registry;
using Messaging.RabbitMq.Connection;
using Messaging.RabbitMq.Consuming;
using Messaging.RabbitMq.Options;
using Messaging.RabbitMq.Topology;
using Microsoft.Extensions.Options;

namespace UserService.HostedService
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
            _logger.LogInformation("UserService Event Consumer Hosted Service starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
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

                    var queues = rabbitOptions
                        .Exchanges
                        .SelectMany(e => e.Queues)
                        .Select(q => q.Name)
                        .Distinct();

                    var tasks = queues.Select(async queue =>
                    {
                        var queueChannel = await connection.CreateChannelAsync();

                        var consumer = new GenericEventConsumer(
                            queueChannel,
                            eventDispatcher,
                            loggerFactory.CreateLogger<GenericEventConsumer>(),
                            eventTypeRegistry);

                        await consumer.StartAsync(queue, stoppingToken);

                        _logger.LogInformation("Started consumer for queue: {Queue}", queue);
                    });

                    await Task.WhenAll(tasks);
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("UserService Event Consumer Hosted Service stopping...");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "UserService Event Consumer Hosted Service could not start. Retrying in 5 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
    }
}
