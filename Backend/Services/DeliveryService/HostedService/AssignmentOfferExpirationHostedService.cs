using DeliveryService.Options;
using DeliveryService.Repositories.Interfaces;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Publishing;
using Microsoft.Extensions.Options;

namespace DeliveryService.HostedService
{
    public class AssignmentOfferExpirationHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<DeliveryOption> _options;
        private readonly ILogger<AssignmentOfferExpirationHostedService> _logger;

        public AssignmentOfferExpirationHostedService(
            IServiceProvider serviceProvider,
            IOptions<DeliveryOption> options,
            ILogger<AssignmentOfferExpirationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
                    var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
                    var now = DateTime.UtcNow;

                    var expiredOffers = await repository.ExpireStaleAssignmentOffersAsync(now, stoppingToken);
                    foreach (var group in expiredOffers.GroupBy(offer => new { offer.OrderId, offer.OrderNumber }))
                    {
                        await publisher.PublishAsync(new AssignmentExpiredEvent
                        {
                            CorrelationId = group.Key.OrderId.ToString(),
                            OrderId = group.Key.OrderId,
                            OrderNumber = group.Key.OrderNumber,
                            Offers = group.Select(offer => new AssignmentExpiredOfferPayload
                            {
                                AssignmentId = offer.Id,
                                OfferId = offer.Id,
                                ShipperId = offer.ShipperId,
                                ExpiredAt = now
                            }).ToArray()
                        });
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while expiring assignment offers");
                }

                var delaySeconds = Math.Max(_options.Value.AssignmentOfferExpirationScanSeconds, 1);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
            }
        }
    }
}
