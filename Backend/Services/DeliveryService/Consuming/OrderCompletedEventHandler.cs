using DeliveryService.Entities;
using DeliveryService.Enums;
using DeliveryService.Options;
using DeliveryService.Repositories.Interfaces;
using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Publishing;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DeliveryService.Consuming
{
    public class OrderCompletedEventHandler : IEventHandler<OrderCompletedEvent>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IRedisRepository _redisRepository;
        private readonly IOptions<DeliveryOption> _deliveryOptions;
        private readonly IEventPublisher _publisher;
        private readonly ILogger<OrderCompletedEventHandler> _logger;

        public OrderCompletedEventHandler(IDeliveryRepository deliveryRepository, IRedisRepository redisRepository, IOptions<DeliveryOption> deliveryOptions, IEventPublisher publisher, ILogger<OrderCompletedEventHandler> logger)
        {
            _deliveryRepository = deliveryRepository;
            _redisRepository = redisRepository;
            _deliveryOptions = deliveryOptions;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task Handle(OrderCompletedEvent @event)
        {
            var merchantLng = (double)(@event.MerchantAddress?.Lng ?? 0m);
            var merchantLat = (double)(@event.MerchantAddress?.Lat ?? 0m);

            var shippers = await _redisRepository.GetShipperLocationInRadiusAsync(
                merchantLng,
                merchantLat,
                _deliveryOptions.Value.FindingShipperRadius,
                _deliveryOptions.Value.GeoUnit);

            var allShippers = await _deliveryRepository.GetAllShipperAvailabilityAsync();

            var shipperMembers = shippers?.Select(s => s.Member).ToHashSet() ?? new HashSet<RedisValue>();

            var availableShipper = allShippers
                .Where(s => s != null && s.Status == ShipperWorkStatus.ActiveIdle && shipperMembers.Contains(s.ShipperId.ToString()))
                .Select(s => s.ShipperId)
                .ToList();

            if (availableShipper.Any())
            {
                var assignments = availableShipper.Select(shipperId => new ShipperAssignment
                {
                    Id = Guid.NewGuid(),
                    OrderId = @event.OrderId,
                    CustomerId = @event.UserId,
                    OrderNumber = @event.OrderNumber,
                    ShipperId = shipperId,
                    Status = AssignmentStatus.Pending,
                    AssignedAt = DateTime.UtcNow
                }).ToList();

                await _deliveryRepository.CreateShipperAssignmentsAsync(assignments);
            }
            else
            {
                _logger.LogWarning("No available shippers found for order {OrderId}", @event.OrderId);
            }

            var shipperFoundEvent = new ShipperFoundEvent()
            {
                OrderId = @event.OrderId,
                OrderNumber = @event.OrderNumber,

                ShipperIds = availableShipper,

                MerchantLng = merchantLng,
                MerchantLat = merchantLat,

                CustomerLng = (double)(@event.DeliveryAddress?.Lng ?? 0m),
                CustomerLat = (double)(@event.DeliveryAddress?.Lat ?? 0m)
            };

            if (availableShipper.Any())
            {
                await _publisher.PublishAsync(shipperFoundEvent);
            }
        }
    }
}
