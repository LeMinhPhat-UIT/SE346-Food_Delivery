using DeliveryService.Entities;

namespace DeliveryService.Services.Interfaces
{
    public interface IOpenRouteServiceClient
    {
        Task<RouteEstimate> EstimateRouteAsync(
            decimal pickupLat,
            decimal pickupLng,
            decimal deliveryLat,
            decimal deliveryLng,
            CancellationToken cancellationToken = default);
    }
}
