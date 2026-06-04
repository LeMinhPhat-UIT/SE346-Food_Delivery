using NotificationService.Entities;

namespace NotificationService.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task CreateNotificationAsync(Notification notification);
        Task<IQueryable<Notification>> GetAllNotificationsByUserIdAsync(Guid userId);
        Task<Notification?> GetNotificationByIdAsync(Guid notificationId);
        Task UpdateNotificationAsync(Notification notification);
        Task<IQueryable<UserDevice>> GetAllUserDevicesAsync();
        Task<IQueryable<UserDevice>> GetAllUserDevicesByUserIdAsync(Guid userId);
        Task<UserDevice?> GetUserDeviceByDeviceTokenAsync(string deviceToken);
        Task CreateUserDeviceAsync(UserDevice userDevice);
        Task UpdateUserDeviceAsync(UserDevice userDevice);
    }
}
