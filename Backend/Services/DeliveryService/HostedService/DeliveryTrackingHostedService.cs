using DeliveryService.Entities;
using DeliveryService.Persistences;
using DeliveryService.Repositories.Interfaces;
using StackExchange.Redis;

namespace DeliveryService.HostedService
{
    public class DeliveryTrackingHostedService : BackgroundService
    {
        private readonly IDatabase _redis;
        private IServiceProvider _serviceProvider;
        private IDeliveryRepository _repository;

        private readonly string GeoKey = "Shipper:Active";

        public DeliveryTrackingHostedService(IConnectionMultiplexer redisConnector, IServiceProvider serviceProvider, IDeliveryRepository repository)
        {
            _redis = redisConnector.GetDatabase();
            _serviceProvider = serviceProvider;
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncLocationToDbAsync();
                }
                catch (Exception ex)
                {

                }

                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }

        private async Task SyncLocationToDbAsync()
        {
            var shipperIds = await _redis.SortedSetRangeByRankAsync(GeoKey);

            if (shipperIds.Length == 0)
                return;

            var positions = await _redis.GeoPositionAsync(GeoKey, shipperIds);

            for (int i = 0; i < shipperIds.Length; i++)
            {
                var pos = positions[i];

                Guid.TryParse(shipperIds[i].ToString(), out var id);

                var shipperAvailability = await _repository.GetShipperAvailabilityByShipperIdAsync(id) as ShipperAvailability;

                await _repository.UpdateShipperAvailabilityAsync(shipperAvailability!);
            }
        }
    }
}
