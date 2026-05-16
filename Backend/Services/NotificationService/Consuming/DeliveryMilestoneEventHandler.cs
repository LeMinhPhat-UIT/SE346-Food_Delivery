using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using NotificationService.Entities;
using NotificationService.Repositories.Interfaces;
using NotificationService.Services.Interfaces;

namespace NotificationService.Consuming
{
    public class DeliveryMilestoneEventHandler : IEventHandler<DeliveryMilestoneEvent>
    {
        private readonly IPushNotificationService _pushNotifcationService;
        private readonly INotificationRepository _notificationRepository;

        public DeliveryMilestoneEventHandler(IPushNotificationService pushNotifcationService, INotificationRepository notificationRepository)
        {
            _pushNotifcationService = pushNotifcationService;
            _notificationRepository = notificationRepository;
        }

        public async Task Handle(DeliveryMilestoneEvent @event)
        {
            var (title, body, type) = @event.Milestone switch
            {
                DeliveryMilestoneType.PickedUp => (
                    "Your order has been picked up",
                    $"Shipper has picked up order #{@event.OrderNumber} and is heading to you",
                    "order_being_shipped"),
                DeliveryMilestoneType.Delivered => (
                    "Your order has arrived",
                    $"Shipper has arrived with order #{@event.OrderNumber}",
                    "order_delivered"),
                _ => (
                    "Delivery update",
                    $"Order #{@event.OrderNumber} has been updated",
                    "order_update")
            };

            var data = new Dictionary<string, string>
            {
                { "OrderId", @event.OrderId.ToString() },
                { "OrderNumber", @event.OrderNumber },
                { "ShipperId", @event.ShipperId.ToString() },
                { "Type", type },
                { "Milestone", @event.Milestone.ToString() }
            };

            if (!string.IsNullOrWhiteSpace(@event.ProofFileKey))
            {
                data["ProofFileKey"] = @event.ProofFileKey;
            }

            await _notificationRepository.CreateNotificationAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = @event.CustomerId,
                Title = title,
                Body = body,
                Type = type,
                ReferenceId = @event.OrderId,
                ReferenceType = "order",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await SendToCustomer(@event.CustomerId, title, body, data);
        }

        private async Task SendToCustomer(Guid customerId, string title, string body, Dictionary<string, string> data)
        {
            var userDevices = await _notificationRepository.GetAllUserDevicesByUserIdAsync(customerId);

            var deviceTokens = new List<string>();
            if (userDevices != null)
            {
                foreach (var ud in userDevices)
                {
                    if (ud != null && !string.IsNullOrWhiteSpace(ud.DeviceToken))
                        deviceTokens.Add(ud.DeviceToken);
                }
            }

            if (deviceTokens.Any())
            {
                var tasks = deviceTokens.Select(dt => _pushNotifcationService.SendNotificationAsync(dt, title, body, data));
                await Task.WhenAll(tasks);
            }
        }
    }
}