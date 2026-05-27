using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;
using NotificationService.Integrations;
using NotificationService.Realtime;

namespace NotificationService.Consuming
{
    public class AssignmentExpiredEventHandler : IEventHandler<AssignmentExpiredEvent>
    {
        private readonly IHubContext<AssignmentHub> _hubContext;
        private readonly IRealtimeConnectionTracker _connectionTracker;
        private readonly IUserServiceClient _userServiceClient;

        public AssignmentExpiredEventHandler(
            IHubContext<AssignmentHub> hubContext,
            IRealtimeConnectionTracker connectionTracker,
            IUserServiceClient userServiceClient)
        {
            _hubContext = hubContext;
            _connectionTracker = connectionTracker;
            _userServiceClient = userServiceClient;
        }

        public async Task Handle(AssignmentExpiredEvent @event)
        {
            foreach (var offer in @event.Offers)
            {
                var payload = new
                {
                    type = "ASSIGNMENT_EXPIRED",
                    assignmentId = offer.AssignmentId,
                    offerId = offer.OfferId,
                    orderId = @event.OrderId,
                    expiredAt = offer.ExpiredAt
                };

                var shipperGroup = RealtimeGroups.Shipper(offer.ShipperId);
                if (_connectionTracker.HasConnections(shipperGroup))
                {
                    await _hubContext.Clients
                        .Group(shipperGroup)
                        .SendAsync("AssignmentExpired", payload);

                    continue;
                }

                var userId = await _userServiceClient.GetUserIdByShipperIdAsync(offer.ShipperId);
                if (userId.HasValue && _connectionTracker.HasConnections(RealtimeGroups.User(userId.Value)))
                {
                    await _hubContext.Clients
                        .Group(RealtimeGroups.User(userId.Value))
                        .SendAsync("AssignmentExpired", payload);
                }
            }
        }
    }
}
