using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using NotificationService.Entities;
using NotificationService.Integrations;
using NotificationService.Repositories.Interfaces;
using NotificationService.Services.Interfaces;

namespace NotificationService.Consuming
{
    public class ShipperFoundEventHandler : IEventHandler<ShipperFoundEvent>
    {
        private readonly IPushNotificationService _pushNotifcationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserServiceClient _userServiceClient;

        public ShipperFoundEventHandler(
            IPushNotificationService pushNotifcationService,
            INotificationRepository notificationRepository,
            IUserServiceClient userServiceClient)
        {
            _pushNotifcationService = pushNotifcationService;
            _notificationRepository = notificationRepository;
            _userServiceClient = userServiceClient;
        }

        public async Task Handle(ShipperFoundEvent @event)
        {
            string TITLE = "Order is ready";
            string BODY = $"Order #{@event.OrderNumber} is done. Come and get it right away";
            var DATA = new Dictionary<string, string>
            {
                { "OrderId", @event.OrderId.ToString() },
                { "Type", "order_pickup" }
            };

            var tasks = @event.ShipperIds
                .Select(item => SendToNearByShipper(item, TITLE, BODY, new Dictionary<string, string>(DATA)))
                .ToArray();

            if (tasks.Any())
            {
                await Task.WhenAll(tasks);
            }
        }

        private async Task SendToNearByShipper(Guid shipperId, string title, string body, Dictionary<string, string> data)
        {
            var userId = await _userServiceClient.GetUserIdByShipperIdAsync(shipperId) ?? shipperId;
            data["ShipperId"] = shipperId.ToString();

            await _notificationRepository.CreateNotificationAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Body = body,
                Type = "order_pickup",
                ReferenceId = null,
                ReferenceType = "order",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            var userDevices = await _notificationRepository.GetAllUserDevicesByUserIdAsync(userId);

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
