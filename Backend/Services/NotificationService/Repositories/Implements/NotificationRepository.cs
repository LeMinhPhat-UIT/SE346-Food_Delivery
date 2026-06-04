using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;
using NotificationService.Persistences;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.Repositories.Implements
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public Task<IQueryable<Notification>> GetAllNotificationsByUserIdAsync(Guid userId)
        {
            IQueryable<Notification> query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .AsNoTracking()
                .AsQueryable();

            return Task.FromResult(query);
        }

        public async Task<Notification?> GetNotificationByIdAsync(Guid notificationId)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);
        }

        public async Task UpdateNotificationAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public Task<IQueryable<UserDevice>> GetAllUserDevicesAsync()
        {
            IQueryable<UserDevice> query = _context.UserDevices
                .Where(ud => ud.DeletedAt == null && ud.IsActive)
                .AsNoTracking()
                .AsQueryable();
            return Task.FromResult(query);
        }

        public Task<IQueryable<UserDevice>> GetAllUserDevicesByUserIdAsync(Guid userId)
        {
            IQueryable<UserDevice> query = _context.UserDevices
                .Where(ud => ud.UserId == userId && ud.DeletedAt == null && ud.IsActive)
                .AsNoTracking()
                .AsQueryable();
            return Task.FromResult(query);
        }

        public async Task<UserDevice?> GetUserDeviceByDeviceTokenAsync(string deviceToken)
        {
            return await _context.UserDevices
                .FirstOrDefaultAsync(ud => ud.DeviceToken == deviceToken);
        }

        public async Task CreateUserDeviceAsync(UserDevice userDevice)
        {
            await _context.UserDevices.AddAsync(userDevice);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserDeviceAsync(UserDevice userDevice)
        {
            _context.UserDevices.Update(userDevice);
            await _context.SaveChangesAsync();
        }
    }
}
