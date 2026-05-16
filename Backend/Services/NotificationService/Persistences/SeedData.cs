using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;
using NotificationService.Enums;

namespace NotificationService.Persistences
{
    public class SeedData
    {
        public static void InitializeData(ModelBuilder modelBuilder)
        {
            var userId = Guid.Parse("71111111-1111-1111-1111-111111111111");
            var notificationId = Guid.Parse("72222222-2222-2222-2222-222222222222");
            var userDeviceId = Guid.Parse("73333333-3333-3333-3333-333333333333");
            var seededAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1, 0, 0, 0), DateTimeKind.Utc);

            modelBuilder.Entity<Notification>().HasData(
                new Notification
                {
                    Id = notificationId,
                    UserId = userId,
                    Title = "Welcome to Food Delivery",
                    Body = "Your account is ready to receive order updates.",
                    Type = "system",
                    ReferenceType = "user",
                    IsRead = false,
                    CreatedAt = seededAt
                }
            );

            modelBuilder.Entity<UserDevice>().HasData(
                new UserDevice
                {
                    Id = userDeviceId,
                    UserId = userId,
                    DeviceToken = "seed-device-token",
                    DeviceType = DeviceType.Android,
                    IsActive = true,
                    CreatedAt = seededAt
                }
            );
        }
    }
}
