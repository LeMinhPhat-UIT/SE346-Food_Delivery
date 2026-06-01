using Microsoft.EntityFrameworkCore;
using UserService.Entities;
using UserService.Enums;

namespace UserService.Persistences
{
    public class Seeding
    {
        public static void InitializeData(ModelBuilder builder)
        {
            var adminUserId = Guid.Parse("55555555-5555-4555-8555-555555555555");
            var customerUserId = Guid.Parse("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa");
            var merchantUserId = Guid.Parse("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb");
            var shipperUserId = Guid.Parse("99999999-9999-4999-9999-999999999999");
            var merchantId = Guid.Parse("cccccccc-cccc-4ccc-8ccc-cccccccccccc");
            var customerAddressId = Guid.Parse("dddddddd-dddd-4ddd-8ddd-dddddddddddd");
            var merchantAddressId = Guid.Parse("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee");
            var merchantStoreAddressId = Guid.Parse("ffffffff-ffff-4fff-8fff-ffffffffffff");
            var shipperAddressId = Guid.Parse("12121212-1212-4212-8212-121212121212");
            var shipperRequestId = Guid.Parse("34343434-3434-4434-8434-343434343434");
            var shipperId = Guid.Parse("56565656-5656-4656-8656-565656565656");
            var seededAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1, 0, 0, 0), DateTimeKind.Utc);

            builder.Entity<User>().HasData(
                new User
                {
                    Id = adminUserId,
                    FullName = "Seeded Admin",
                    AvatarUrl = "https://example.com/avatars/admin.png",
                    PhoneNumber = "0900000000",
                    Status = UserStatus.Active,
                    CreatedAt = seededAt
                },
                new User
                {
                    Id = customerUserId,
                    FullName = "Seeded Customer",
                    AvatarUrl = "https://example.com/avatars/customer.png",
                    PhoneNumber = "0900000001",
                    Status = UserStatus.Active,
                    CreatedAt = seededAt
                },
                new User
                {
                    Id = merchantUserId,
                    FullName = "Seeded Merchant Owner",
                    AvatarUrl = "https://example.com/avatars/merchant.png",
                    PhoneNumber = "0900000002",
                    Status = UserStatus.Active,
                    CreatedAt = seededAt
                },
                new User
                {
                    Id = shipperUserId,
                    FullName = "Seeded Shipper",
                    AvatarUrl = "https://example.com/avatars/shipper.png",
                    PhoneNumber = "0900000003",
                    Status = UserStatus.Active,
                    CreatedAt = seededAt
                }
            );

            builder.Entity<Address>().HasData(
                new Address
                {
                    Id = customerAddressId,
                    UserId = customerUserId,
                    Label = "Home",
                    RecipientName = "Seeded Customer",
                    Phone = "0900000001",
                    AddressLine = "1 Nguyen Hue",
                    Ward = "Ben Nghe",
                    District = "District 1",
                    City = "Ho Chi Minh City",
                    Lat = 10.7769m,
                    Lng = 106.7009m,
                    IsDefault = true,
                    CreatedAt = seededAt
                },
                new Address
                {
                    Id = merchantAddressId,
                    UserId = merchantUserId,
                    Label = "Store Contact",
                    RecipientName = "Seeded Merchant Owner",
                    Phone = "0900000002",
                    AddressLine = "2 Le Loi",
                    Ward = "Ben Thanh",
                    District = "District 1",
                    City = "Ho Chi Minh City",
                    Lat = 10.7722m,
                    Lng = 106.6983m,
                    IsDefault = true,
                    CreatedAt = seededAt
                },
                new Address
                {
                    Id = shipperAddressId,
                    UserId = shipperUserId,
                    Label = "Home",
                    RecipientName = "Seeded Shipper",
                    Phone = "0900000003",
                    AddressLine = "3 Nguyen Trai",
                    Ward = "Pham Ngu Lao",
                    District = "District 1",
                    City = "Ho Chi Minh City",
                    Lat = 10.7691m,
                    Lng = 106.6824m,
                    IsDefault = true,
                    CreatedAt = seededAt
                }
            );

            builder.Entity<Merchant>().HasData(
                new Merchant
                {
                    Id = merchantId,
                    UserId = merchantUserId,
                    StoreName = "Seeded Merchant Store",
                    StoreDescription = "Default merchant store for local development.",
                    StoreLogoUrl = "https://example.com/stores/logo.png",
                    StoreBannerUrl = "https://example.com/stores/banner.png",
                    BusinessLicense = "BL-SEED-0001",
                    TaxId = "TAX-SEED-0001",
                    IsOpen = true,
                    OpeningTime = new TimeSpan(8, 0, 0),
                    ClosingTime = new TimeSpan(22, 0, 0),
                    MinOrderAmount = 30000m,
                    AvgPrepTime = 20,
                    Status = MerchantStatus.Approved,
                    CreatedAt = seededAt
                }
            );

            builder.Entity<MerchantAddress>().HasData(
                new MerchantAddress
                {
                    Id = merchantStoreAddressId,
                    MerchantId = merchantId,
                    AddressLine = "2 Le Loi",
                    Ward = "Ben Thanh",
                    District = "District 1",
                    City = "Ho Chi Minh City",
                    Lat = 10.7722m,
                    Lng = 106.6983m,
                    CreatedAt = seededAt
                }
            );

            builder.Entity<ShipperRequest>().HasData(
                new ShipperRequest
                {
                    Id = shipperRequestId,
                    UserId = shipperUserId,
                    LicenseNumber = "DL-SEED-0001",
                    LicenseFrontUrl = "https://example.com/shipper/license-front.jpg",
                    LicenseBackUrl = "https://example.com/shipper/license-back.jpg",
                    IdCardFrontUrl = "https://example.com/shipper/id-front.jpg",
                    IdCardBackUrl = "https://example.com/shipper/id-back.jpg",
                    SelfieUrl = "https://example.com/shipper/selfie.jpg",
                    IdNumber = "079202600001",
                    FullName = "Seeded Shipper",
                    DateOfBirth = new DateTime(1998, 4, 12, 0, 0, 0, DateTimeKind.Utc),
                    VerificationStatus = VerificationStatus.Approved,
                    RejectedReason = string.Empty,
                    VerifiedAt = seededAt.AddDays(1),
                    CreatedAt = seededAt,
                    ReviewedBy = merchantUserId
                }
            );

            builder.Entity<Shipper>().HasData(
                new Shipper
                {
                    Id = shipperId,
                    UserId = shipperUserId,
                    VehiclePlate = "59A-123.45",
                    Status = ShipperStatus.Approved,
                    RequestId = shipperRequestId,
                    CreatedAt = seededAt.AddDays(1)
                }
            );
        }
    }
}
