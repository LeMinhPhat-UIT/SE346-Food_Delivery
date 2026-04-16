namespace NotificationService.Senders.Interfaces
{
    public interface ISenderProvider
    {
        Task SendAsync(object content);
    }
}
