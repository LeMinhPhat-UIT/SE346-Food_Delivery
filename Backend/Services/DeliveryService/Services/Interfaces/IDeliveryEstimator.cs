using DeliveryService.Entities;

namespace DeliveryService.Services.Interfaces
{
    public interface IDeliveryEstimator
    {
        Task<DeliveryFeeEstimate> EstimateAsync(
            DeliveryFeeEstimateInput input,
            CancellationToken cancellationToken = default);
    }
}
