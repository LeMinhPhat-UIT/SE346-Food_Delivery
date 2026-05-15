using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using NotificationService.Repositories.Interfaces;
using NotificationService.Services.Interfaces;

namespace NotificationService.Consuming
{
    public class MerchantRequestReviewedEventHandler : IEventHandler<MerchantRequestReviewedEvent>
    {
        private readonly IPushNotificationService _pushNotifcationService;
        private readonly INotificationRepository _notificationRepository;

        public MerchantRequestReviewedEventHandler(IPushNotificationService pushNotifcationService, INotificationRepository notificationRepository)
        {
            _pushNotifcationService = pushNotifcationService;
            _notificationRepository = notificationRepository;
        }

        public async Task Handle(MerchantRequestReviewedEvent @event)
        {
            string TITLE = "Merchant request reviewed";
            string BODY = $"Request #{@event.RequestId} had been reviewed.";
            var DATA = new Dictionary<string, string>
            {
                { "RequestId", @event.RequestId.ToString() },
                { "ReviewerId", @event.ReviewerId.ToString() },
                { "IsApproved", @event.IsApproved.ToString() },
                { "RejectedReason", @event.RejectedReason },
                { "Type", "request_reviewed" }
            };

            await SendToMerchant(@event.UserId, TITLE, BODY, DATA);
        }

        private async Task SendToMerchant(Guid userId, string title, string body, Dictionary<string, string> data)
        {
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
