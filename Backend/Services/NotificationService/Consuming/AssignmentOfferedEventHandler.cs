using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Entities;
using NotificationService.Hubs;
using NotificationService.Integrations;
using NotificationService.Realtime;
using NotificationService.Repositories.Interfaces;
using NotificationService.Services.Interfaces;

namespace NotificationService.Consuming
{
    public class AssignmentOfferedEventHandler : IEventHandler<AssignmentOfferedEvent>
    {
        private readonly IHubContext<AssignmentHub> _hubContext;
        private readonly IRealtimeConnectionTracker _connectionTracker;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserServiceClient _userServiceClient;

        public AssignmentOfferedEventHandler(
            IHubContext<AssignmentHub> hubContext,
            IRealtimeConnectionTracker connectionTracker,
            IPushNotificationService pushNotificationService,
            INotificationRepository notificationRepository,
            IUserServiceClient userServiceClient)
        {
            _hubContext = hubContext;
            _connectionTracker = connectionTracker;
            _pushNotificationService = pushNotificationService;
            _notificationRepository = notificationRepository;
            _userServiceClient = userServiceClient;
        }

        public async Task Handle(AssignmentOfferedEvent @event)
        {
            foreach (var offer in @event.Offers)
            {
                var userId = await _userServiceClient.GetUserIdByShipperIdAsync(offer.ShipperId) ?? offer.ShipperId;
                var title = "New delivery assignment";
                var body = $"Order #{@event.OrderNumber} is ready for pickup.";
                var data = new Dictionary<string, string>
                {
                    ["Type"] = "ASSIGNMENT_OFFERED",
                    ["AssignmentId"] = offer.AssignmentId.ToString(),
                    ["OfferId"] = offer.OfferId.ToString(),
                    ["OrderId"] = @event.OrderId.ToString(),
                    ["ShipperId"] = offer.ShipperId.ToString(),
                    ["ExpiresAt"] = offer.ExpiresAt.ToString("O")
                };

                await _notificationRepository.CreateNotificationAsync(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = title,
                    Body = body,
                    Type = "assignment",
                    ReferenceId = offer.OfferId,
                    ReferenceType = "delivery_assignment",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                var realtimePayload = new
                {
                    type = "ASSIGNMENT_OFFERED",
                    assignmentId = offer.AssignmentId,
                    offerId = offer.OfferId,
                    orderId = @event.OrderId,
                    merchant = new
                    {
                        id = @event.MerchantId,
                        name = @event.MerchantName,
                        address = @event.PickupLocation.Address,
                        latitude = @event.PickupLocation.Latitude,
                        longitude = @event.PickupLocation.Longitude
                    },
                    dropoff = new
                    {
                        address = @event.DropoffLocation.Address,
                        latitude = @event.DropoffLocation.Latitude,
                        longitude = @event.DropoffLocation.Longitude
                    },
                    estimatedDistanceToMerchantKm = offer.EstimatedDistanceToMerchantKm,
                    estimatedDeliveryDistanceKm = offer.EstimatedDeliveryDistanceKm,
                    estimatedFee = offer.EstimatedFee,
                    expiresAt = offer.ExpiresAt
                };

                var shipperGroup = RealtimeGroups.Shipper(offer.ShipperId);
                var userGroup = RealtimeGroups.User(userId);
                if (_connectionTracker.HasConnections(shipperGroup))
                {
                    await _hubContext.Clients.Group(shipperGroup).SendAsync("AssignmentOffered", realtimePayload);
                    continue;
                }

                if (_connectionTracker.HasConnections(userGroup))
                {
                    await _hubContext.Clients.Group(userGroup).SendAsync("AssignmentOffered", realtimePayload);
                    continue;
                }

                await SendPushAsync(userId, title, body, data);
            }
        }

        private async Task SendPushAsync(Guid userId, string title, string body, Dictionary<string, string> data)
        {
            var userDevices = await _notificationRepository.GetAllUserDevicesByUserIdAsync(userId);
            var tokens = userDevices
                .Where(device => !string.IsNullOrWhiteSpace(device.DeviceToken))
                .Select(device => device.DeviceToken)
                .ToArray();

            if (tokens.Length == 0)
                return;

            await Task.WhenAll(tokens.Select(token => _pushNotificationService.SendNotificationAsync(token, title, body, data)));
        }
    }
}
