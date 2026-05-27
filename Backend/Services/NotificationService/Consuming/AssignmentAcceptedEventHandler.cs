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
    public class AssignmentAcceptedEventHandler : IEventHandler<AssignmentAcceptedEvent>
    {
        private readonly IHubContext<AssignmentHub> _hubContext;
        private readonly IRealtimeConnectionTracker _connectionTracker;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserServiceClient _userServiceClient;

        public AssignmentAcceptedEventHandler(
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

        public async Task Handle(AssignmentAcceptedEvent @event)
        {
            await NotifyAcceptedShipperAsync(@event);
            await NotifyCancelledShippersAsync(@event);
            await NotifyCustomerAsync(@event);
        }

        private async Task NotifyAcceptedShipperAsync(AssignmentAcceptedEvent @event)
        {
            var userId = await _userServiceClient.GetUserIdByShipperIdAsync(@event.AcceptedByShipperId) ?? @event.AcceptedByShipperId;
            var payload = new
            {
                type = "ASSIGNMENT_ACCEPTED",
                assignmentId = @event.AssignmentId,
                offerId = @event.OfferId,
                orderId = @event.OrderId
            };

            await DeliverToShipperAsync(
                userId,
                @event.AcceptedByShipperId,
                "Assignment accepted",
                $"You accepted order #{@event.OrderNumber}.",
                "AssignmentAccepted",
                payload,
                new Dictionary<string, string>
                {
                    ["Type"] = "ASSIGNMENT_ACCEPTED",
                    ["AssignmentId"] = @event.AssignmentId.ToString(),
                    ["OfferId"] = @event.OfferId.ToString(),
                    ["OrderId"] = @event.OrderId.ToString()
                });
        }

        private async Task NotifyCancelledShippersAsync(AssignmentAcceptedEvent @event)
        {
            foreach (var shipperId in @event.CancelledShipperIds)
            {
                var userId = await _userServiceClient.GetUserIdByShipperIdAsync(shipperId) ?? shipperId;
                var payload = new
                {
                    type = "ASSIGNMENT_TAKEN",
                    assignmentId = @event.AssignmentId,
                    orderId = @event.OrderId,
                    message = "This assignment has already been accepted by another shipper."
                };

                await DeliverToShipperAsync(
                    userId,
                    shipperId,
                    "Assignment taken",
                    "This assignment has already been accepted by another shipper.",
                    "AssignmentTaken",
                    payload,
                    new Dictionary<string, string>
                    {
                        ["Type"] = "ASSIGNMENT_TAKEN",
                        ["AssignmentId"] = @event.AssignmentId.ToString(),
                        ["OrderId"] = @event.OrderId.ToString()
                    });
            }
        }

        private async Task NotifyCustomerAsync(AssignmentAcceptedEvent @event)
        {
            await _notificationRepository.CreateNotificationAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = @event.CustomerId,
                Title = "Shipper accepted your order",
                Body = $"A shipper accepted order #{@event.OrderNumber}.",
                Type = "order_update",
                ReferenceId = @event.OrderId,
                ReferenceType = "order",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await SendPushAsync(
                @event.CustomerId,
                "Shipper accepted your order",
                $"A shipper accepted order #{@event.OrderNumber}.",
                new Dictionary<string, string>
                {
                    ["Type"] = "ORDER_SHIPPER_ACCEPTED",
                    ["OrderId"] = @event.OrderId.ToString(),
                    ["AssignmentId"] = @event.AssignmentId.ToString()
                });
        }

        private async Task DeliverToShipperAsync(
            Guid userId,
            Guid shipperId,
            string title,
            string body,
            string methodName,
            object realtimePayload,
            Dictionary<string, string> data)
        {
            await _notificationRepository.CreateNotificationAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Body = body,
                Type = "assignment",
                ReferenceId = Guid.TryParse(data.GetValueOrDefault("AssignmentId"), out var assignmentId) ? assignmentId : null,
                ReferenceType = "delivery_assignment",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            var shipperGroup = RealtimeGroups.Shipper(shipperId);
            var userGroup = RealtimeGroups.User(userId);
            if (_connectionTracker.HasConnections(shipperGroup))
            {
                await _hubContext.Clients.Group(shipperGroup).SendAsync(methodName, realtimePayload);
                return;
            }

            if (_connectionTracker.HasConnections(userGroup))
            {
                await _hubContext.Clients.Group(userGroup).SendAsync(methodName, realtimePayload);
                return;
            }

            await SendPushAsync(userId, title, body, data);
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
