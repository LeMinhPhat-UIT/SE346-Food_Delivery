using DeliveryService.DTOs;
using DeliveryService.Entities;
using DeliveryService.Hubs.Interfaces;
using DeliveryService.Repositories.Interfaces;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Publishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace DeliveryService.Hubs.Implements
{
    [Authorize]
    public class TrackingHub : Hub<ITrackingHub>
    {
        private readonly IRedisRepository _redisRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<TrackingHub> _logger;

        public TrackingHub(IRedisRepository redisRepository, IEventPublisher eventPublisher, ILogger<TrackingHub> logger)
        {
            _redisRepository = redisRepository;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task JoinOrderGroup(Guid orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, orderId.ToString());
        }
        public async Task LeaveOrderGroup(Guid orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, orderId.ToString());
        }
        public async Task SendLocation(UpdateLocationRequest request)
        {
            if (request.OrderId == Guid.Empty || request.ShipperId == Guid.Empty)
            {
                _logger.LogWarning("Rejected invalid location update for connection {ConnectionId}", Context.ConnectionId);
                return;
            }

            await _redisRepository.UpdateShipperLocationAsync(new ShipperAvailability
            {
                ShipperId = request.ShipperId,
                CurrentLat = request.Latitude,
                CurrentLng = request.Longitude
            });

            await Clients.OthersInGroup(request.OrderId.ToString()).ReceiveLocation(request);

            await _eventPublisher.PublishAsync<ShipperLocationUpdatedEvent>(new ShipperLocationUpdatedEvent()
            {
                Longitude = request.Longitude,
                Latitude = request.Latitude,
                OrderId = request.OrderId,
                ShipperId = request.ShipperId,
            });
        }
    }
}
