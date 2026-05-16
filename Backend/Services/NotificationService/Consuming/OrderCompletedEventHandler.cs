using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Common;
using Messaging.Contracts.Events;
using NotificationService.Entities;
using NotificationService.Repositories.Interfaces;
using NotificationService.Services.Interfaces;

namespace NotificationService.Consuming
{
    public class OrderCompletedEventHandler : IEventHandler<OrderCompletedEvent>
    {
        private readonly IPushNotificationService _pushNotifcationService;
        private readonly INotificationRepository _notificationRepository;

        public OrderCompletedEventHandler(IPushNotificationService pushNotifcationService, INotificationRepository notificationRepository)
        {
            _pushNotifcationService = pushNotifcationService;
            _notificationRepository = notificationRepository;
        }

        public async Task Handle(OrderCompletedEvent @event)
        {
            string TITLE = "Your order is done";
            string BODY = $"Merchant has done order {@event.OrderId}, shipper is on the way";
            var DATA = new Dictionary<string, string>
            {
                { "OrderId", @event.OrderId.ToString() },
                { "Type", "order_update" },
                { "Status", @event.OrderStatus.ToString() }
            };

            await _notificationRepository.CreateNotificationAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = @event.UserId,
                Title = TITLE,
                Body = BODY,
                Type = "order_update",
                ReferenceId = @event.OrderId,
                ReferenceType = "order",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await SendToCustomer(@event.UserId, TITLE, BODY, DATA);
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
