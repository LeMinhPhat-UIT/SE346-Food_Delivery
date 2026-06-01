using DeliveryService.DTOs;
using DeliveryService.Entities;
using Riok.Mapperly.Abstractions;

namespace DeliveryService.Mappers
{
    [Mapper]
    public partial class DeliveryMapper
    {
        public DeliveryFeeEstimateInput ToDeliveryFeeEstimateInput(EstimateDeliveryFeeRequest request)
        {
            return new DeliveryFeeEstimateInput
            {
                OrderId = request.OrderId,
                PickupLat = request.PickupLat.GetValueOrDefault(),
                PickupLng = request.PickupLng.GetValueOrDefault(),
                DeliveryLat = request.DeliveryLat.GetValueOrDefault(),
                DeliveryLng = request.DeliveryLng.GetValueOrDefault(),
                Subtotal = request.Subtotal.GetValueOrDefault(),
                IsRushHour = request.IsRushHour.GetValueOrDefault()
            };
        }

        public EstimateDeliveryFeeResponse ToEstimateDeliveryFeeResponse(DeliveryFeeEstimate estimate)
        {
            return new EstimateDeliveryFeeResponse
            {
                QuoteId = estimate.QuoteId,
                OrderId = estimate.OrderId,
                PickupLat = estimate.PickupLat,
                PickupLng = estimate.PickupLng,
                DeliveryLat = estimate.DeliveryLat,
                DeliveryLng = estimate.DeliveryLng,
                DistanceKm = estimate.DistanceKm,
                Subtotal = estimate.Subtotal,
                EstimatedTimeMinutes = estimate.EstimatedTimeMinutes,
                BaseFee = estimate.BaseFee,
                DistanceFee = estimate.DistanceFee,
                SmallOrderSurcharge = estimate.SmallOrderSurcharge,
                RushHourSurcharge = estimate.RushHourSurcharge,
                RawFee = estimate.RawFee,
                DeliveryFee = estimate.DeliveryFee,
                Currency = estimate.Currency,
                IsSmallOrder = estimate.IsSmallOrder,
                IsRushHour = estimate.IsRushHour,
                IsWithinDeliveryRadius = estimate.IsWithinDeliveryRadius,
                MaxDeliveryDistanceKm = estimate.MaxDeliveryDistanceKm,
                PolicyBreakdowns = estimate.PolicyBreakdowns
                    .Select(detail => new DeliveryFeePolicyBreakdownResponse
                    {
                        PolicyId = detail.PolicyId,
                        PolicyName = detail.PolicyName,
                        BaseFee = detail.BaseFee,
                        DistanceFee = detail.DistanceFee,
                        SmallOrderSurcharge = detail.SmallOrderSurcharge,
                        RushHourSurcharge = detail.RushHourSurcharge,
                        RawFee = detail.RawFee,
                        FinalFee = detail.FinalFee,
                        IsSmallOrder = detail.IsSmallOrder,
                        IsRushHour = detail.IsRushHour
                    })
                    .ToList()
            };
        }
    }
}
