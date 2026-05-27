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
    public class OrderReadyForPickupEventHandler :
        IEventHandler<OrderReadyForPickupEvent>,
        IEventHandler<OrderCompletedEvent>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IRedisRepository _redisRepository;
        private readonly IOptions<DeliveryOption> _deliveryOptions;
        private readonly IDeliveryEstimator _deliveryEstimator;
        private readonly IEventPublisher _publisher;
        private readonly ILogger<OrderReadyForPickupEventHandler> _logger;

        public OrderReadyForPickupEventHandler(
            IDeliveryRepository deliveryRepository,
            IRedisRepository redisRepository,
            IOptions<DeliveryOption> deliveryOptions,
            IDeliveryEstimator deliveryEstimator,
            IEventPublisher publisher,
            ILogger<OrderReadyForPickupEventHandler> logger)
        {
            _deliveryRepository = deliveryRepository;
            _redisRepository = redisRepository;
            _deliveryOptions = deliveryOptions;
            _deliveryEstimator = deliveryEstimator;
            _publisher = publisher;
            _logger = logger;
        }

        public Task Handle(OrderReadyForPickupEvent @event)
        {
            return DispatchAsync(new ReadyForPickupPayload
            {
                OrderId = @event.OrderId,
                OrderNumber = @event.OrderNumber,
                MerchantId = @event.MerchantId,
                MerchantStoreName = @event.MerchantStoreName,
                MerchantAddress = @event.MerchantAddress,
                CustomerId = @event.UserId,
                DeliveryAddress = @event.DeliveryAddress,
                DeliveryFee = @event.DeliveryFee,
                DistanceKm = @event.DistanceKm,
                CorrelationId = @event.CorrelationId
            });
        }

        public Task Handle(OrderCompletedEvent @event)
        {
            return DispatchAsync(new ReadyForPickupPayload
            {
                OrderId = @event.OrderId,
                OrderNumber = @event.OrderNumber,
                MerchantId = @event.MerchantId,
                MerchantStoreName = @event.MerchantStoreName,
                MerchantAddress = @event.MerchantAddress,
                CustomerId = @event.UserId,
                DeliveryAddress = @event.DeliveryAddress,
                DeliveryFee = @event.DeliveryFee,
                DistanceKm = @event.DistanceKm,
                CorrelationId = @event.CorrelationId
            });
        }

        private async Task DispatchAsync(ReadyForPickupPayload payload)
        {
            var options = _deliveryOptions.Value;
            var now = DateTime.UtcNow;
            var expiresAt = now.AddSeconds(Math.Max(options.AssignmentOfferTimeoutSeconds, 1));

            var merchantLngDecimal = payload.MerchantAddress?.Lng ?? 0m;
            var merchantLatDecimal = payload.MerchantAddress?.Lat ?? 0m;
            var customerLngDecimal = payload.DeliveryAddress?.Lng ?? 0m;
            var customerLatDecimal = payload.DeliveryAddress?.Lat ?? 0m;
            var merchantLng = (double)merchantLngDecimal;
            var merchantLat = (double)merchantLatDecimal;

            var distanceKm = payload.DistanceKm;
            var deliveryFee = payload.DeliveryFee;

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
                options.FindingShipperRadius,
                options.RedisGeoUnit);

            var shipperMembers = shippers?.Select(s => s.Member).ToHashSet() ?? new HashSet<RedisValue>();
            if (shipperMembers.Count == 0)
            {
                _logger.LogWarning("No nearby shipper locations found for order {OrderId}", payload.OrderId);
                return;
            }

            var stalenessCutoff = now.AddSeconds(-Math.Max(options.AllowedLocationStalenessSeconds, 1));
            var maxShippers = Math.Max(options.MaxShippersPerBatch, 1);
            var allShippers = await _deliveryRepository.GetAllShipperAvailabilityAsync();
            var candidates = allShippers
                .AsEnumerable()
                .Where(s => s != null &&
                            s.Status == ShipperWorkStatus.ActiveIdle &&
                            s.CurrentOfferedAssignmentId == null &&
                            s.LastSeenAt.HasValue &&
                            s.LastSeenAt.Value >= stalenessCutoff &&
                            shipperMembers.Contains(s.ShipperId.ToString()))
                .Take(maxShippers)
                .ToList();

            var offeredAssignments = new List<ShipperAssignment>();

            foreach (var candidate in candidates)
            {
                var assignment = new ShipperAssignment
                {
                    Id = Guid.NewGuid(),
                    OrderId = payload.OrderId,
                    CustomerId = payload.CustomerId,
                    MerchantId = payload.MerchantId,
                    OrderNumber = payload.OrderNumber,
                    ShipperId = candidate.ShipperId,
                    MerchantName = payload.MerchantStoreName,
                    PickupAddress = payload.MerchantAddress?.AddressLine ?? string.Empty,
                    PickupLatitude = merchantLatDecimal,
                    PickupLongitude = merchantLngDecimal,
                    DropoffAddress = payload.DeliveryAddress?.AddressLine ?? string.Empty,
                    DropoffLatitude = customerLatDecimal,
                    DropoffLongitude = customerLngDecimal,
                    DeliveryFee = deliveryFee,
                    DistanceKm = distanceKm,
                    AssignedAt = now,
                    OfferExpiresAt = expiresAt
                };

                if (await _deliveryRepository.TryCreateAssignmentOfferAsync(assignment, expiresAt))
                    offeredAssignments.Add(assignment);
            }

            if (offeredAssignments.Count == 0)
            {
                _logger.LogWarning("No available shippers could be locked for order {OrderId}", payload.OrderId);
                return;
            }

            await _publisher.PublishAsync(new AssignmentOfferedEvent
            {
                CorrelationId = payload.CorrelationId ?? payload.OrderId.ToString(),
                OrderId = payload.OrderId,
                OrderNumber = payload.OrderNumber,
                CustomerId = payload.CustomerId,
                MerchantId = payload.MerchantId,
                MerchantName = payload.MerchantStoreName,
                PickupLocation = new LocationPayload
                {
                    Address = payload.MerchantAddress?.AddressLine ?? string.Empty,
                    Latitude = merchantLatDecimal,
                    Longitude = merchantLngDecimal
                },
                DropoffLocation = new LocationPayload
                {
                    Address = payload.DeliveryAddress?.AddressLine ?? string.Empty,
                    Latitude = customerLatDecimal,
                    Longitude = customerLngDecimal
                },
                Offers = offeredAssignments.Select(assignment => new AssignmentOfferPayload
                {
                    AssignmentId = assignment.Id,
                    OfferId = assignment.Id,
                    ShipperId = assignment.ShipperId,
                    EstimatedDistanceToMerchantKm = 0m,
                    EstimatedDeliveryDistanceKm = assignment.DistanceKm,
                    EstimatedFee = assignment.DeliveryFee,
                    ExpiresAt = expiresAt
                }).ToArray()
            });
        }

        private sealed class ReadyForPickupPayload
        {
            public Guid OrderId { get; init; }
            public string OrderNumber { get; init; } = string.Empty;
            public Guid MerchantId { get; init; }
            public string MerchantStoreName { get; init; } = string.Empty;
            public MerchantAddressBaseDto? MerchantAddress { get; init; }
            public Guid CustomerId { get; init; }
            public UserAddressBaseDto? DeliveryAddress { get; init; }
            public decimal DeliveryFee { get; init; }
            public decimal DistanceKm { get; init; }
            public string? CorrelationId { get; init; }
        }
    }
}
