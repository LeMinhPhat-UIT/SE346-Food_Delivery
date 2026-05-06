using NotificationService.Services.Interfaces;
using FirebaseAdmin.Messaging;

namespace NotificationService.Services.Implements
{
    public class PushNotificationService : IPushNotificationService
    {
        public async Task<string> SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string> data)
        {
            var message = new Message()
            {
                Token = deviceToken,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body,
                },
                Data = data
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return response;
        }
    }
}
