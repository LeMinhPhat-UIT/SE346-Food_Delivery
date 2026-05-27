using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;
using NotificationService.Integrations;
using NotificationService.Realtime;

namespace NotificationService.Consuming
{
    public class AssignmentRejectedEventHandler : IEventHandler<AssignmentRejectedEvent>
    {
        private readonly IHubContext<AssignmentHub> _hubContext;
        private readonly IRealtimeConnectionTracker _connectionTracker;
        private readonly IUserServiceClient _userServiceClient;

        public AssignmentRejectedEventHandler(
            IHubContext<AssignmentHub> hubContext,
            IRealtimeConnectionTracker connectionTracker,
            IUserServiceClient userServiceClient)
        {
            _hubContext = hubContext;
            _connectionTracker = connectionTracker;
            _userServiceClient = userServiceClient;
        }

        public async Task Handle(AssignmentRejectedEvent @event)
        {
            var payload = new
            {
                type = "ASSIGNMENT_REJECTED",
                assignmentId = @event.AssignmentId,
                offerId = @event.OfferId,
                orderId = @event.OrderId,
                reason = @event.Reason,
                rejectedAt = @event.RejectedAt
            };

            var shipperGroup = RealtimeGroups.Shipper(@event.ShipperId);
            if (_connectionTracker.HasConnections(shipperGroup))
            {
                await _hubContext.Clients.Group(shipperGroup).SendAsync("AssignmentRejected", payload);
                return;
            }

            var userId = await _userServiceClient.GetUserIdByShipperIdAsync(@event.ShipperId);
            if (userId.HasValue && _connectionTracker.HasConnections(RealtimeGroups.User(userId.Value)))
                await _hubContext.Clients.Group(RealtimeGroups.User(userId.Value)).SendAsync("AssignmentRejected", payload);
        }
    }
}
