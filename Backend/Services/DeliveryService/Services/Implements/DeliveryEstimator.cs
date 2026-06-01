using DeliveryService.Entities;
using DeliveryService.Exceptions;
using DeliveryService.Options;
using DeliveryService.Repositories.Interfaces;
using DeliveryService.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DeliveryService.Services.Implements
{
    public class DeliveryEstimator : IDeliveryEstimator
    {
        private readonly IOpenRouteServiceClient _routeServiceClient;
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IOptions<DeliveryOption> _deliveryOptions;

        public DeliveryEstimator(
            IOpenRouteServiceClient routeServiceClient,
            IDeliveryRepository deliveryRepository,
            IOptions<DeliveryOption> deliveryOptions)
        {
            _routeServiceClient = routeServiceClient;
            _deliveryRepository = deliveryRepository;
            _deliveryOptions = deliveryOptions;
        }

        public async Task<DeliveryFeeEstimate> EstimateAsync(
            DeliveryFeeEstimateInput input,
            CancellationToken cancellationToken = default)
        {
            var distanceKm = input.DistanceKm.GetValueOrDefault();
            var durationSeconds = 0d;

            if (distanceKm <= 0m)
            {
                var route = await _routeServiceClient.EstimateRouteAsync(
                    input.PickupLat,
                    input.PickupLng,
                    input.DeliveryLat,
                    input.DeliveryLng,
                    cancellationToken);

                distanceKm = route.DistanceKm;
                durationSeconds = route.DurationSeconds;
            }

            var roundedDistanceKm = RoundDistance(Math.Max(distanceKm, 0m));
            var subtotal = RoundMoney(Math.Max(input.Subtotal, 0m));
            var policyBreakdowns = await CalculateActivePolicyBreakdownsAsync(
                roundedDistanceKm,
                subtotal,
                input.IsRushHour,
                cancellationToken);

            var options = _deliveryOptions.Value;
            var maxDeliveryDistanceKm = options.DeliveryRadius > 0 ? (decimal)options.DeliveryRadius : 0m;
            var quoteId = input.PersistQuote ? Guid.NewGuid() : (Guid?)null;
            var createdAt = DateTime.UtcNow;

            var estimate = new DeliveryFeeEstimate
            {
                QuoteId = quoteId,
                OrderId = input.OrderId,
                PickupLat = input.PickupLat,
                PickupLng = input.PickupLng,
                DeliveryLat = input.DeliveryLat,
                DeliveryLng = input.DeliveryLng,
                DistanceKm = roundedDistanceKm,
                Subtotal = subtotal,
                EstimatedTimeMinutes = CalculateEstimatedTimeMinutes(durationSeconds),
                BaseFee = RoundMoney(policyBreakdowns.Sum(detail => detail.BaseFee)),
                DistanceFee = RoundMoney(policyBreakdowns.Sum(detail => detail.DistanceFee)),
                SmallOrderSurcharge = RoundMoney(policyBreakdowns.Sum(detail => detail.SmallOrderSurcharge)),
                RushHourSurcharge = RoundMoney(policyBreakdowns.Sum(detail => detail.RushHourSurcharge)),
                RawFee = RoundMoney(policyBreakdowns.Sum(detail => detail.RawFee)),
                DeliveryFee = RoundMoney(policyBreakdowns.Sum(detail => detail.FinalFee)),
                Currency = string.IsNullOrWhiteSpace(options.Currency) ? "VND" : options.Currency,
                IsSmallOrder = policyBreakdowns.Any(detail => detail.IsSmallOrder),
                IsRushHour = input.IsRushHour,
                IsWithinDeliveryRadius = options.DeliveryRadius <= 0 || roundedDistanceKm <= maxDeliveryDistanceKm,
                MaxDeliveryDistanceKm = maxDeliveryDistanceKm,
                PolicyBreakdowns = policyBreakdowns
            };

            if (input.PersistQuote)
                await PersistQuoteAsync(estimate, createdAt, cancellationToken);

            return estimate;
        }

        private async Task<List<DeliveryFeePolicyFeeBreakdown>> CalculateActivePolicyBreakdownsAsync(
            decimal distanceKm,
            decimal subtotal,
            bool isRushHour,
            CancellationToken cancellationToken)
        {
            var policies = await _deliveryRepository.GetActiveDeliveryFeePoliciesWithTiersAsync(cancellationToken);

            if (policies.Count == 0)
                throw new DeliveryFeePolicyException("No active delivery fee policies are configured");

            return policies
                .Select(policy => CalculatePolicyBreakdown(policy, distanceKm, subtotal, isRushHour))
                .ToList();
        }

        private static DeliveryFeePolicyFeeBreakdown CalculatePolicyBreakdown(
            DeliveryFeePolicy policy,
            decimal distanceKm,
            decimal subtotal,
            bool isRushHour)
        {
            var baseFee = RoundMoney(Math.Max(policy.BaseFee, 0m));
            var distanceFee = RoundMoney(CalculateDistanceFee(distanceKm, policy.DistanceTiers));
            var isSmallOrder = policy.SmallOrderThreshold.HasValue && subtotal < policy.SmallOrderThreshold.Value;
            var smallOrderSurcharge = isSmallOrder ? RoundMoney(Math.Max(policy.SmallOrderSurcharge, 0m)) : 0m;
            var rushHourSurcharge = isRushHour ? RoundMoney(Math.Max(policy.RushHourSurcharge, 0m)) : 0m;
            var rawFee = RoundMoney(baseFee + distanceFee + smallOrderSurcharge + rushHourSurcharge);
            var finalFee = ApplyMinMax(rawFee, policy.MinFee, policy.MaxFee);

            return new DeliveryFeePolicyFeeBreakdown
            {
                PolicyId = policy.Id,
                PolicyName = policy.Name,
                BaseFee = baseFee,
                DistanceFee = distanceFee,
                SmallOrderSurcharge = smallOrderSurcharge,
                RushHourSurcharge = rushHourSurcharge,
                RawFee = rawFee,
                FinalFee = finalFee,
                IsSmallOrder = isSmallOrder,
                IsRushHour = isRushHour
            };
        }

        private static decimal CalculateDistanceFee(
            decimal distanceKm,
            IEnumerable<DeliveryFeeDistanceTier> distanceTiers)
        {
            var remainingDistance = Math.Max(distanceKm, 0m);
            if (remainingDistance == 0m)
                return 0m;

            var distanceFee = 0m;

            foreach (var tier in distanceTiers.OrderBy(tier => tier.FromKm))
            {
                var fromKm = Math.Max(tier.FromKm, 0m);
                var toKm = tier.ToKm;
                if (remainingDistance <= fromKm)
                    continue;

                var chargedUntilKm = toKm.HasValue
                    ? Math.Min(remainingDistance, toKm.Value)
                    : remainingDistance;

                if (chargedUntilKm <= fromKm)
                    continue;

                var chargedDistance = chargedUntilKm - fromKm;
                distanceFee += chargedDistance * Math.Max(tier.FeePerKm, 0m);
            }

            return RoundMoney(distanceFee);
        }

        private static decimal ApplyMinMax(decimal rawFee, decimal? minFee, decimal? maxFee)
        {
            var finalFee = rawFee;

            if (minFee.HasValue)
                finalFee = Math.Max(finalFee, RoundMoney(Math.Max(minFee.Value, 0m)));

            if (maxFee.HasValue)
                finalFee = Math.Min(finalFee, RoundMoney(Math.Max(maxFee.Value, 0m)));

            return RoundMoney(finalFee);
        }

        private async Task PersistQuoteAsync(
            DeliveryFeeEstimate estimate,
            DateTime createdAt,
            CancellationToken cancellationToken)
        {
            if (!estimate.QuoteId.HasValue)
                return;

            var quote = new DeliveryFeeQuote
            {
                Id = estimate.QuoteId.Value,
                OrderId = estimate.OrderId,
                PickupLat = estimate.PickupLat,
                PickupLng = estimate.PickupLng,
                DropoffLat = estimate.DeliveryLat,
                DropoffLng = estimate.DeliveryLng,
                DistanceKm = estimate.DistanceKm,
                Subtotal = estimate.Subtotal,
                DeliveryFee = estimate.DeliveryFee,
                Currency = estimate.Currency,
                IsRushHour = estimate.IsRushHour,
                CreatedAt = createdAt,
                Details = estimate.PolicyBreakdowns.Select(detail => new DeliveryFeeQuoteDetail
                {
                    Id = Guid.NewGuid(),
                    QuoteId = estimate.QuoteId.Value,
                    PolicyId = detail.PolicyId,
                    PolicyName = detail.PolicyName,
                    BaseFee = detail.BaseFee,
                    DistanceFee = detail.DistanceFee,
                    SmallOrderSurcharge = detail.SmallOrderSurcharge,
                    RushHourSurcharge = detail.RushHourSurcharge,
                    RawFee = detail.RawFee,
                    FinalFee = detail.FinalFee,
                    IsSmallOrder = detail.IsSmallOrder,
                    IsRushHour = detail.IsRushHour,
                    CreatedAt = createdAt
                }).ToList()
            };

            await _deliveryRepository.CreateDeliveryFeeQuoteAsync(quote, cancellationToken);
        }

        private static int CalculateEstimatedTimeMinutes(double durationSeconds)
        {
            if (durationSeconds <= 0d)
                return 0;

            return Math.Max(1, (int)Math.Ceiling(durationSeconds / 60d));
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
