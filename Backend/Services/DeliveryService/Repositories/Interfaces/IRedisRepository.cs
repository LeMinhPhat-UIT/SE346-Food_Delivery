using DeliveryService.Entities;
using StackExchange.Redis;

namespace DeliveryService.Repositories.Interfaces
{
    public interface IRedisRepository
    {
        Task<bool> UpdateShipperLocationAsync(ShipperAvailability shipperAvailability);
        Task<bool> DeleteShipperLocationAsync(Guid shipperId);
        Task<IEnumerable<GeoRadiusResult>> GetShipperLocationInRadiusAsync(double lng, double lat, double radius, GeoUnit geoUnit);
    }
}
