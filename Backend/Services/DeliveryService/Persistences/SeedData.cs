using DeliveryService.Entities;
using DeliveryService.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Persistences
{
    public class SeedData
    {
        public static void InitializeData(ModelBuilder modelBuilder)
        {
            var trackingId = Guid.Parse("61111111-1111-4111-8111-111111111111");
            var orderId = Guid.Parse("62222222-2222-4222-8222-222222222222");
            var shipperId = Guid.Parse("56565656-5656-4656-8656-565656565656");
            var assignmentId = Guid.Parse("64444444-4444-4444-8444-444444444444");
            var locationId = Guid.Parse("65555555-5555-4555-8555-555555555555");
            var availabilityId = Guid.Parse("66666666-6666-4666-8666-666666666666");
            var customerId = Guid.Parse("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa");
            var merchantId = Guid.Parse("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb");
            var secondTrackingId = Guid.Parse("74444444-4444-4444-8444-444444444444");
            var secondOrderId = Guid.Parse("75555555-5555-4555-8555-555555555555");
            var secondLocationId = Guid.Parse("76666666-6666-4666-8666-666666666666");
            var incidentId = Guid.Parse("77777777-7777-4777-8777-777777777777");
            var defaultFeePolicyId = Guid.Parse("81111111-1111-4111-8111-111111111111");
            var defaultTierOneId = Guid.Parse("82222222-2222-4222-8222-222222222222");
            var defaultTierTwoId = Guid.Parse("83333333-3333-4333-8333-333333333333");
            var defaultTierThreeId = Guid.Parse("84444444-4444-4444-8444-444444444444");
            var defaultTierFourId = Guid.Parse("85555555-5555-4555-8555-555555555555");
            var seededAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1, 0, 0, 0), DateTimeKind.Utc);

            modelBuilder.Entity<DeliveryFeePolicy>().HasData(
                new DeliveryFeePolicy
                {
                    Id = defaultFeePolicyId,
                    Name = "Default Delivery Fee Policy",
                    BaseFee = 10000m,
                    MinFee = 10000m,
                    MaxFee = 60000m,
                    SmallOrderThreshold = 50000m,
                    SmallOrderSurcharge = 5000m,
                    RushHourSurcharge = 5000m,
                    IsActive = true,
                    CreatedAt = seededAt
                }
            );

            modelBuilder.Entity<DeliveryFeeDistanceTier>().HasData(
                new DeliveryFeeDistanceTier
                {
                    Id = defaultTierOneId,
                    PolicyId = defaultFeePolicyId,
                    FromKm = 0m,
                    ToKm = 2m,
                    FeePerKm = 0m
                },
                new DeliveryFeeDistanceTier
                {
                    Id = defaultTierTwoId,
                    PolicyId = defaultFeePolicyId,
                    FromKm = 2m,
                    ToKm = 5m,
                    FeePerKm = 4000m
                },
                new DeliveryFeeDistanceTier
                {
                    Id = defaultTierThreeId,
                    PolicyId = defaultFeePolicyId,
                    FromKm = 5m,
                    ToKm = 10m,
                    FeePerKm = 5000m
                },
                new DeliveryFeeDistanceTier
                {
                    Id = defaultTierFourId,
                    PolicyId = defaultFeePolicyId,
                    FromKm = 10m,
                    ToKm = null,
                    FeePerKm = 6000m
                }
            );

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
                },
                new DeliveryTracking
                {
                    Id = secondTrackingId,
                    OrderId = secondOrderId,
                    ShipperId = shipperId,
                    PickupLat = 10.7821m,
                    PickupLng = 106.6925m,
                    DeliveryLat = 10.7770m,
                    DeliveryLng = 106.7002m,
                    DistanceKm = 1.8m,
                    EstimatedTime = 12,
                    ActualTime = 14,
                    Status = DeliveryStatus.Delivered,
                    CreatedAt = seededAt.AddHours(-1)
                }
            );

            modelBuilder.Entity<ShipperAssignment>().HasData(
                new ShipperAssignment
                {
                    Id = assignmentId,
                    OrderId = orderId,
                    CustomerId = customerId,
                    MerchantId = merchantId,
                    OrderNumber = "ORD-SEED-0001",
                    ShipperId = shipperId,
                    CustomerName = "Seeded Customer",
                    CustomerPhone = "0900000001",
                    MerchantName = "Seed Merchant",
                    PickupAddress = "Seed pickup address",
                    PickupLatitude = 10.7769m,
                    PickupLongitude = 106.7009m,
                    DropoffAddress = "Seed dropoff address",
                    DropoffLatitude = 10.7700m,
                    DropoffLongitude = 106.6950m,
                    DeliveryFee = 21000m,
                    DistanceKm = 2.2m,
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
                },
                new ShipperLocationHistory
                {
                    Id = secondLocationId,
                    OrderId = secondOrderId,
                    ShipperId = shipperId,
                    Latitude = 10.7785m,
                    Longitude = 106.6990m,
                    RecordedAt = seededAt.AddMinutes(25),
                    CorrelationId = "seed-delivery-completed"
                }
            );

            modelBuilder.Entity<ShipperAvailability>().HasData(
                new ShipperAvailability
                {
                    Id = availabilityId,
                    ShipperId = shipperId,
                    Status = ShipperWorkStatus.Delivering,
                    CurrentOrderId = orderId,
                    CurrentAssignmentId = assignmentId,
                    CurrentLat = 10.7735m,
                    CurrentLng = 106.6975m,
                    LastSeenAt = seededAt.AddMinutes(5),
                    CreatedAt = seededAt
                }
            );

            modelBuilder.Entity<Incident>().HasData(
                new Incident
                {
                    Id = incidentId,
                    OrderId = secondOrderId,
                    ReportedBy = customerId,
                    Type = IncidentType.MissingItem,
                    Description = "Customer reported one missing item from the delivered order.",
                    ProofUrl = new[]
                    {
                        "deliveries/75555555-5555-4555-8555-555555555555/56565656-5656-4656-8656-565656565656/incident/order-2-photo-1.jpg",
                        "deliveries/75555555-5555-4555-8555-555555555555/56565656-5656-4656-8656-565656565656/incident/order-2-photo-2.jpg"
                    },
                    Status = IncidentStatus.Investigating,
                    Resolution = "Awaiting support review and customer confirmation.",
                    ResolvedBy = null,
                    ResolvedAt = null,
                    CreatedAt = seededAt.AddHours(-1)
                }
            );
        }
    }
}
