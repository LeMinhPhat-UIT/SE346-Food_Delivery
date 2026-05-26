using DeliveryService.Entities;
using DeliveryService.Options;
using DeliveryService.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DeliveryService.Services.Implements
{
    public class DeliveryEstimator : IDeliveryEstimator
    {
        private readonly IOpenRouteServiceClient _routeServiceClient;
        private readonly IOptions<DeliveryOption> _deliveryOptions;

        public DeliveryEstimator(
            IOpenRouteServiceClient routeServiceClient,
            IOptions<DeliveryOption> deliveryOptions)
        {
            _routeServiceClient = routeServiceClient;
            _deliveryOptions = deliveryOptions;
        }

        public async Task<DeliveryFeeEstimate> EstimateAsync(
            DeliveryFeeEstimateInput input,
            CancellationToken cancellationToken = default)
        {
            var route = await _routeServiceClient.EstimateRouteAsync(
                input.PickupLat,
                input.PickupLng,
                input.DeliveryLat,
                input.DeliveryLng,
                cancellationToken);

            var options = _deliveryOptions.Value;
            var baseFee = RoundMoney(Math.Max(options.BaseDeliveryFee, 0m));
            var feePerKm = Math.Max(options.FeePerKm, 0m);
            var distanceFee = RoundMoney(route.DistanceKm * feePerKm);
            var deliveryFee = EstimateDeliveryFee(route.DistanceKm);
            var maxDeliveryDistanceKm = options.DeliveryRadius > 0 ? (decimal)options.DeliveryRadius : 0m;

            return new DeliveryFeeEstimate
            {
                PickupLat = input.PickupLat,
                PickupLng = input.PickupLng,
                DeliveryLat = input.DeliveryLat,
                DeliveryLng = input.DeliveryLng,
                DistanceKm = RoundDistance(route.DistanceKm),
                EstimatedTimeMinutes = CalculateEstimatedTimeMinutes(route.DurationSeconds),
                BaseFee = baseFee,
                DistanceFee = distanceFee,
                DeliveryFee = deliveryFee,
                Currency = string.IsNullOrWhiteSpace(options.Currency) ? "VND" : options.Currency,
                IsWithinDeliveryRadius = options.DeliveryRadius <= 0 || route.DistanceKm <= maxDeliveryDistanceKm,
                MaxDeliveryDistanceKm = maxDeliveryDistanceKm
            };
        }

        public decimal EstimateDeliveryFee(decimal distanceKm)
        {
            var options = _deliveryOptions.Value;
            var baseFee = RoundMoney(Math.Max(options.BaseDeliveryFee, 0m));
            var feePerKm = Math.Max(options.FeePerKm, 0m);
            var minimumFee = RoundMoney(Math.Max(options.MinimumDeliveryFee, 0m));
            var distanceFee = RoundMoney(Math.Max(distanceKm, 0m) * feePerKm);

            return RoundMoney(Math.Max(baseFee + distanceFee, minimumFee));
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
