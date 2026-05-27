using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Integrations;
using NotificationService.Realtime;
using System.Security.Claims;

namespace NotificationService.Hubs
{
    [Authorize]
    public class AssignmentHub : Hub
    {
        private readonly IRealtimeConnectionTracker _connectionTracker;
        private readonly IUserServiceClient _userServiceClient;

        public AssignmentHub(
            IRealtimeConnectionTracker connectionTracker,
            IUserServiceClient userServiceClient)
        {
            _connectionTracker = connectionTracker;
            _userServiceClient = userServiceClient;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
                await AddToTrackedGroupAsync(RealtimeGroups.User(userId.Value));

            var shipperId = GetCurrentShipperId();
            if (!shipperId.HasValue && userId.HasValue)
                shipperId = await _userServiceClient.GetShipperIdByUserIdAsync(userId.Value);

            if (shipperId.HasValue)
                await AddToTrackedGroupAsync(RealtimeGroups.Shipper(shipperId.Value));

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _connectionTracker.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        private async Task AddToTrackedGroupAsync(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _connectionTracker.Add(groupName, Context.ConnectionId);
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Context.User?.FindFirstValue("sub")
                ?? Context.User?.FindFirstValue("userId");

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private Guid? GetCurrentShipperId()
        {
            var shipperIdClaim = Context.User?.FindFirstValue("shipperId")
                ?? Context.User?.FindFirstValue("ShipperId")
                ?? Context.User?.FindFirstValue("shipper_id");

            return Guid.TryParse(shipperIdClaim, out var shipperId) ? shipperId : null;
        }
    }
}
