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
        private const double EarthRadiusKm = 6371.0088d;
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
            var customerLng = (double)(@event.DeliveryAddress?.Lng ?? 0m);
            var customerLat = (double)(@event.DeliveryAddress?.Lat ?? 0m);
            var distanceKm = @event.DistanceKm > 0m
                ? @event.DistanceKm
                : RoundDistance(CalculateDistanceKm(merchantLat, merchantLng, customerLat, customerLng));
            var deliveryFee = @event.DeliveryFee > 0m
                ? @event.DeliveryFee
                : EstimateDeliveryFee(distanceKm);

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

                CustomerLng = (double)(@event.DeliveryAddress?.Lng ?? 0m),
                CustomerLat = (double)(@event.DeliveryAddress?.Lat ?? 0m)
            };

            if (availableShipper.Any())
            {
                await _publisher.PublishAsync(shipperFoundEvent);
            }
        }

        private decimal EstimateDeliveryFee(decimal distanceKm)
        {
            var options = _deliveryOptions.Value;
            var baseFee = RoundMoney(Math.Max(options.BaseDeliveryFee, 0m));
            var feePerKm = Math.Max(options.FeePerKm, 0m);
            var minimumFee = RoundMoney(Math.Max(options.MinimumDeliveryFee, 0m));
            var distanceFee = RoundMoney(distanceKm * feePerKm);

            return RoundMoney(Math.Max(baseFee + distanceFee, minimumFee));
        }

        private static decimal CalculateDistanceKm(double pickupLat, double pickupLng, double deliveryLat, double deliveryLng)
        {
            var pickupLatRadians = ToRadians(pickupLat);
            var pickupLngRadians = ToRadians(pickupLng);
            var deliveryLatRadians = ToRadians(deliveryLat);
            var deliveryLngRadians = ToRadians(deliveryLng);

            var latDelta = deliveryLatRadians - pickupLatRadians;
            var lngDelta = deliveryLngRadians - pickupLngRadians;

            var a = Math.Pow(Math.Sin(latDelta / 2), 2)
                + Math.Cos(pickupLatRadians) * Math.Cos(deliveryLatRadians) * Math.Pow(Math.Sin(lngDelta / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return (decimal)(EarthRadiusKm * c);
        }

        private static double ToRadians(double degree)
        {
            return degree * Math.PI / 180d;
        }

        private static decimal RoundDistance(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static decimal RoundMoney(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
