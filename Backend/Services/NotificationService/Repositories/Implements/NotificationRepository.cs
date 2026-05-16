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

        public Task<IQueryable<UserDevice>> GetAllUserDevicesAsync()
        {
            IQueryable<UserDevice> query = _context.UserDevices.Where(ud => ud.DeletedAt == null).AsNoTracking().AsQueryable();
            return Task.FromResult(query);
        }

        public Task<IQueryable<UserDevice>> GetAllUserDevicesByUserIdAsync(Guid userId)
        {
            IQueryable<UserDevice> query = _context.UserDevices.Where(ud => ud.UserId == userId && ud.DeletedAt == null).AsNoTracking().AsQueryable();
            return Task.FromResult(query);
        }
    }
}
