namespace NotificationService.Services.Interfaces
{
    public interface IPushNotificationService
    {
        Task<string> SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string> data);
    }
}
