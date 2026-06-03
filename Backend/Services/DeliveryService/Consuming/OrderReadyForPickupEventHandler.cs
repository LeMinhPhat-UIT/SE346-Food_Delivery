using DeliveryService.Entities;
using DeliveryService.Enums;
using DeliveryService.Options;
using DeliveryService.Repositories.Interfaces;
using DeliveryService.Services.Interfaces;
using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Publishing;
using Microsoft.EntityFrameworkCore;
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
                CustomerName = @event.CustomerName,
                CustomerPhone = @event.CustomerPhone,
                DeliveryAddress = @event.DeliveryAddress,
                TotalAmount = @event.TotalAmount,
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
                CustomerName = @event.CustomerName,
                CustomerPhone = @event.CustomerPhone,
                DeliveryAddress = @event.DeliveryAddress,
                TotalAmount = @event.TotalAmount,
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
                    OrderId = payload.OrderId,
                    PickupLat = merchantLatDecimal,
                    PickupLng = merchantLngDecimal,
                    DeliveryLat = customerLatDecimal,
                    DeliveryLng = customerLngDecimal,
                    Subtotal = payload.TotalAmount,
                    PersistQuote = deliveryFee <= 0m
                });

                distanceKm = estimate.DistanceKm;

                if (deliveryFee <= 0m)
                    deliveryFee = estimate.DeliveryFee;
            }
            else if (deliveryFee <= 0m)
            {
                var estimate = await _deliveryEstimator.EstimateAsync(new DeliveryFeeEstimateInput
                {
                    OrderId = payload.OrderId,
                    PickupLat = merchantLatDecimal,
                    PickupLng = merchantLngDecimal,
                    DeliveryLat = customerLatDecimal,
                    DeliveryLng = customerLngDecimal,
                    DistanceKm = distanceKm,
                    Subtotal = payload.TotalAmount
                });

                deliveryFee = estimate.DeliveryFee;
            }

            var shippers = (await _redisRepository.GetShipperLocationInRadiusAsync(
                merchantLng,
                merchantLat,
                options.FindingShipperRadius,
                options.RedisGeoUnit)).ToArray();

            if (shippers.Length == 0)
            {
                _logger.LogWarning("No nearby shipper locations found for order {OrderId}", payload.OrderId);
                return;
            }

            var nearbyShipperIds = shippers
                .Select(s => s.Member.ToString())
                .Where(member => TryParseShipperId(member, out _))
                .Select(member => Guid.Parse(member))
                .Distinct()
                .ToArray();

            if (nearbyShipperIds.Length == 0)
            {
                _logger.LogWarning(
                    "Nearby Redis shipper entries did not contain valid shipper ids for order {OrderId}. Members: {Members}",
                    payload.OrderId,
                    string.Join(", ", shippers.Select(s => s.Member.ToString())));
                return;
            }

            var stalenessCutoff = now.AddSeconds(-Math.Max(options.AllowedLocationStalenessSeconds, 1));
            var maxShippers = Math.Max(options.MaxShippersPerBatch, 1);
            var allShippers = await _deliveryRepository.GetAllShipperAvailabilityAsync();
            var candidateQuery = allShippers
                .Where(s => s.Status == ShipperWorkStatus.ActiveIdle &&
                            s.CurrentOrderId == null &&
                            s.CurrentAssignmentId == null &&
                            s.CurrentOfferedAssignmentId == null &&
                            nearbyShipperIds.Contains(s.ShipperId));

            var candidates = await candidateQuery
                .Where(s => s.LastSeenAt.HasValue && s.LastSeenAt.Value >= stalenessCutoff)
                .OrderByDescending(s => s.LastSeenAt)
                .Take(maxShippers)
                .ToListAsync();

            if (candidates.Count == 0)
            {
                _logger.LogInformation(
                    "No nearby shipper availability rows passed the staleness cutoff for order {OrderId}; using Redis proximity matches.",
                    payload.OrderId);

                candidates = await candidateQuery
                    .OrderByDescending(s => s.LastSeenAt)
                    .Take(maxShippers)
                    .ToListAsync();
            }

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
                    CustomerName = payload.CustomerName,
                    CustomerPhone = payload.CustomerPhone,
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
                CustomerName = payload.CustomerName,
                CustomerPhone = payload.CustomerPhone,
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
            public string CustomerName { get; init; } = string.Empty;
            public string CustomerPhone { get; init; } = string.Empty;
            public UserAddressBaseDto? DeliveryAddress { get; init; }
            public decimal TotalAmount { get; init; }
            public decimal DeliveryFee { get; init; }
            public decimal DistanceKm { get; init; }
            public string? CorrelationId { get; init; }
        }

        private static bool TryParseShipperId(string? value, out Guid shipperId)
        {
            shipperId = default;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Guid.TryParse(value.Trim(), out shipperId);
        }
    }
}
