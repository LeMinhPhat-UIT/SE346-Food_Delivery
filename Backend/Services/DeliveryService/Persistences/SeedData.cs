using DeliveryService.Entities;
using DeliveryService.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Persistences
{
    public class SeedData
    {
        public static void InitializeData(ModelBuilder modelBuilder)
        {
            var trackingId = Guid.Parse("61111111-1111-1111-1111-111111111111");
            var orderId = Guid.Parse("62222222-2222-2222-2222-222222222222");
            var shipperId = Guid.Parse("63333333-3333-3333-3333-333333333333");
            var assignmentId = Guid.Parse("64444444-4444-4444-4444-444444444444");
            var locationId = Guid.Parse("65555555-5555-5555-5555-555555555555");
            var availabilityId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var customerId = Guid.Parse("67777777-7777-7777-7777-777777777777");
            var seededAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1, 0, 0, 0), DateTimeKind.Utc);

            modelBuilder.Entity<DeliveryTracking>().HasData(
                new DeliveryTracking
                {
                    Id = trackingId,
                    OrderId = orderId,
                    ShipperId = shipperId,
                    PickupLat = 10.7769m,
                    PickupLng = 106.7009m,
                    DeliveryLat = 10.7700m,
                    DeliveryLng = 106.6950m,
                    DistanceKm = 2.2m,
                    EstimatedTime = 15,
                    Status = DeliveryStatus.Assigned,
                    CreatedAt = seededAt
                }
            );

            modelBuilder.Entity<ShipperAssignment>().HasData(
                new ShipperAssignment
                {
                    Id = assignmentId,
                    OrderId = orderId,
                    CustomerId = customerId,
                    OrderNumber = "ORD-SEED-0001",
                    ShipperId = shipperId,
                    Status = AssignmentStatus.Accepted,
                    AssignedAt = seededAt,
                    AcceptedAt = seededAt.AddMinutes(1)
                }
            );

            modelBuilder.Entity<ShipperLocationHistory>().HasData(
                new ShipperLocationHistory
                {
                    Id = locationId,
                    OrderId = orderId,
                    ShipperId = shipperId,
                    Latitude = 10.7735m,
                    Longitude = 106.6975m,
                    RecordedAt = seededAt.AddMinutes(5),
                    CorrelationId = "seed-delivery"
                }
            );

            modelBuilder.Entity<ShipperAvailability>().HasData(
                new ShipperAvailability
                {
                    Id = availabilityId,
                    ShipperId = shipperId,
                    Status = ShipperWorkStatus.Delivering,
                    CurrentOrderId = orderId,
                    CurrentLat = 10.7735m,
                    CurrentLng = 106.6975m,
                    LastSeenAt = seededAt.AddMinutes(5),
                    CreatedAt = seededAt
                }
            );
        }
    }
}
