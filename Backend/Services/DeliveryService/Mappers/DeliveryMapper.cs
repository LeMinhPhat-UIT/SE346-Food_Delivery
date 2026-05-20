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
                PickupLat = request.PickupLat.GetValueOrDefault(),
                PickupLng = request.PickupLng.GetValueOrDefault(),
                DeliveryLat = request.DeliveryLat.GetValueOrDefault(),
                DeliveryLng = request.DeliveryLng.GetValueOrDefault()
            };
        }

        public partial EstimateDeliveryFeeResponse ToEstimateDeliveryFeeResponse(DeliveryFeeEstimate estimate);
    }
}
