using DeliveryService.Entities;

namespace DeliveryService.Repositories.Interfaces
{
    public interface IDeliveryRepository
    {
        Task<IQueryable<ShipperAvailability>> GetAllShipperAvailabilityAsync();
        Task UpdateShipperAvailabilityAsync(ShipperAvailability shipperAvailability);
        Task<ShipperAvailability?> GetShipperAvailabilityByShipperIdAsync(Guid shipperId);
        Task AddShipperLocationHistoriesAsync(IEnumerable<ShipperLocationHistory> histories, CancellationToken cancellationToken = default);
    }
}
