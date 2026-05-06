using DeliveryService.Entities;
using DeliveryService.Repositories.Interfaces;
using StackExchange.Redis;

namespace DeliveryService.Repositories.Implements
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IDatabase _redis;

        public RedisRepository(IConnectionMultiplexer redisConnector)
        {
            _redis = redisConnector.GetDatabase();
        }

        public async Task<bool> UpdateShipperLocationAsync(ShipperAvailability shipperAvailability)
        {
            return await _redis.GeoAddAsync("Shipper:Active", ((double)shipperAvailability.CurrentLng), ((double)shipperAvailability.CurrentLat), shipperAvailability.ShipperId.ToString());
        }

        public async Task<bool> DeleteShipperLocationAsync(Guid shipperId)
        {
            return await _redis.GeoRemoveAsync("Shipper:Active", shipperId.ToString());
        }

        public async Task<IEnumerable<GeoRadiusResult>> GetShipperLocationInRadiusAsync(double lng, double lat, double radius, GeoUnit geoUnit)
        {
            return await _redis.GeoRadiusAsync("Shipper:Active", lng, lat, radius, geoUnit);
        }
    }
}
