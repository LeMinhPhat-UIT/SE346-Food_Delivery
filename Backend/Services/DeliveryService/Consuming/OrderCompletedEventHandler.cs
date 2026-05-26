using DeliveryService.Entities;
using DeliveryService.Enums;
using DeliveryService.Options;
using DeliveryService.Repositories.Interfaces;
using DeliveryService.Services.Interfaces;
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
        private readonly IDeliveryEstimator _deliveryEstimator;
        private readonly IEventPublisher _publisher;
        private readonly ILogger<OrderCompletedEventHandler> _logger;

        public OrderCompletedEventHandler(
            IDeliveryRepository deliveryRepository,
            IRedisRepository redisRepository,
            IOptions<DeliveryOption> deliveryOptions,
            IDeliveryEstimator deliveryEstimator,
            IEventPublisher publisher,
            ILogger<OrderCompletedEventHandler> logger)
        {
            _deliveryRepository = deliveryRepository;
            _redisRepository = redisRepository;
            _deliveryOptions = deliveryOptions;
            _deliveryEstimator = deliveryEstimator;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task Handle(OrderCompletedEvent @event)
        {
            var merchantLngDecimal = @event.MerchantAddress?.Lng ?? 0m;
            var merchantLatDecimal = @event.MerchantAddress?.Lat ?? 0m;
            var customerLngDecimal = @event.DeliveryAddress?.Lng ?? 0m;
            var customerLatDecimal = @event.DeliveryAddress?.Lat ?? 0m;
            var merchantLng = (double)merchantLngDecimal;
            var merchantLat = (double)merchantLatDecimal;
            var customerLng = (double)customerLngDecimal;
            var customerLat = (double)customerLatDecimal;

            var distanceKm = @event.DistanceKm;
            var deliveryFee = @event.DeliveryFee;

            if (distanceKm <= 0m)
            {
                var estimate = await _deliveryEstimator.EstimateAsync(new DeliveryFeeEstimateInput
                {
                    PickupLat = merchantLatDecimal,
                    PickupLng = merchantLngDecimal,
                    DeliveryLat = customerLatDecimal,
                    DeliveryLng = customerLngDecimal
                });

                distanceKm = estimate.DistanceKm;

                if (deliveryFee <= 0m)
                    deliveryFee = estimate.DeliveryFee;
            }
            else if (deliveryFee <= 0m)
            {
                deliveryFee = _deliveryEstimator.EstimateDeliveryFee(distanceKm);
            }

            var shippers = await _redisRepository.GetShipperLocationInRadiusAsync(
                merchantLng,
                merchantLat,
                _deliveryOptions.Value.FindingShipperRadius,
                _deliveryOptions.Value.RedisGeoUnit);

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
                    MerchantId = @event.MerchantId,
                    OrderNumber = @event.OrderNumber,
                    ShipperId = shipperId,
                    DeliveryFee = deliveryFee,
                    DistanceKm = distanceKm,
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

                CustomerLng = customerLng,
                CustomerLat = customerLat
            };

            if (availableShipper.Any())
            {
                await _publisher.PublishAsync(shipperFoundEvent);
            }
        }

    }
}
