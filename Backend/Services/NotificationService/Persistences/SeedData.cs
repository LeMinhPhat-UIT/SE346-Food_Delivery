using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;
using NotificationService.Enums;

namespace NotificationService.Persistences
{
    public class SeedData
    {
        public static void InitializeData(ModelBuilder modelBuilder)
        {
            var customerUserId = Guid.Parse("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa");
            var merchantUserId = Guid.Parse("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb");
            var shipperUserId = Guid.Parse("99999999-9999-4999-9999-999999999999");
            var notificationId = Guid.Parse("72222222-2222-4222-8222-222222222222");
            var userDeviceId = Guid.Parse("73333333-3333-4333-8333-333333333333");
            var notificationUpdateId = Guid.Parse("74444444-4444-4444-8444-444444444444");
            var notificationShipperId = Guid.Parse("75555555-5555-4555-8555-555555555555");
            var merchantDeviceId = Guid.Parse("76666666-6666-4666-8666-666666666666");
            var shipperDeviceId = Guid.Parse("77777777-7777-4777-8777-777777777777");
            var seededAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1, 0, 0, 0), DateTimeKind.Utc);

            modelBuilder.Entity<Notification>().HasData(
                new Notification
                {
                    Id = notificationId,
                    UserId = customerUserId,
                    Title = "Welcome to Food Delivery",
                    Body = "Your account is ready to receive order updates.",
                    Type = "system",
                    ReferenceType = "user",
                    IsRead = false,
                    CreatedAt = seededAt
                },
                new Notification
                {
                    Id = notificationUpdateId,
                    UserId = customerUserId,
                    Title = "Your order is being prepared",
                    Body = "The merchant has confirmed the order and it is being prepared for delivery.",
                    Type = "order_update",
                    ReferenceId = Guid.Parse("62222222-2222-4222-8222-222222222222"),
                    ReferenceType = "delivery_tracking",
                    IsRead = true,
                    CreatedAt = seededAt.AddMinutes(10)
                },
                new Notification
                {
                    Id = notificationShipperId,
                    UserId = shipperUserId,
                    Title = "New delivery assignment",
                    Body = "You have a new order waiting for pickup.",
                    Type = "assignment",
                    ReferenceId = Guid.Parse("64444444-4444-4444-8444-444444444444"),
                    ReferenceType = "delivery_assignment",
                    IsRead = false,
                    CreatedAt = seededAt.AddMinutes(15)
                }
            );

            modelBuilder.Entity<UserDevice>().HasData(
                new UserDevice
                {
                    Id = userDeviceId,
                    UserId = customerUserId,
                    DeviceToken = "seed-device-token",
                    DeviceType = DeviceType.Android,
                    IsActive = true,
                    CreatedAt = seededAt
                },
                new UserDevice
                {
                    Id = merchantDeviceId,
                    UserId = merchantUserId,
                    DeviceToken = "seed-merchant-device-token",
                    DeviceType = DeviceType.Web,
                    IsActive = true,
                    CreatedAt = seededAt
                },
                new UserDevice
                {
                    Id = shipperDeviceId,
                    UserId = shipperUserId,
                    DeviceToken = "seed-shipper-device-token",
                    DeviceType = DeviceType.Ios,
                    IsActive = true,
                    CreatedAt = seededAt
                }
            );
        }
    }
}
