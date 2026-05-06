using NotificationService.Entities;

namespace NotificationService.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<IQueryable<UserDevice>> GetAllUserDevicesAsync();
        Task<IQueryable<UserDevice>> GetAllUserDevicesByUserIdAsync(Guid userId);
    }
}
