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
            var merchantTwoUserId = Guid.Parse("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2");
            var merchantThreeUserId = Guid.Parse("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3");
            var shipperUserId = Guid.Parse("99999999-9999-4999-9999-999999999999");
            var merchantId = Guid.Parse("cccccccc-cccc-4ccc-8ccc-cccccccccccc");
            var merchantTwoId = Guid.Parse("c2c2c2c2-c2c2-4c2c-8c2c-c2c2c2c2c2c2");
            var merchantThreeId = Guid.Parse("c3c3c3c3-c3c3-4c3c-8c3c-c3c3c3c3c3c3");
            var customerAddressId = Guid.Parse("dddddddd-dddd-4ddd-8ddd-dddddddddddd");
            var merchantAddressId = Guid.Parse("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee");
            var merchantTwoAddressId = Guid.Parse("d2d2d2d2-d2d2-4d2d-8d2d-d2d2d2d2d2d2");
            var merchantThreeAddressId = Guid.Parse("d3d3d3d3-d3d3-4d3d-8d3d-d3d3d3d3d3d3");
            var merchantStoreAddressId = Guid.Parse("ffffffff-ffff-4fff-8fff-ffffffffffff");
            var merchantTwoStoreAddressId = Guid.Parse("e2e2e2e2-e2e2-4e2e-8e2e-e2e2e2e2e2e2");
            var merchantThreeStoreAddressId = Guid.Parse("e3e3e3e3-e3e3-4e3e-8e3e-e3e3e3e3e3e3");
            var merchantRequestId = Guid.Parse("a1a1a1a1-a1a1-4a1a-8a1a-a1a1a1a1a1a1");
            var merchantTwoRequestId = Guid.Parse("a2a2a2a2-a2a2-4a2a-8a2a-a2a2a2a2a2a2");
            var merchantThreeRequestId = Guid.Parse("a3a3a3a3-a3a3-4a3a-8a3a-a3a3a3a3a3a3");
            var shipperAddressId = Guid.Parse("12121212-1212-4212-8212-121212121212");
            var shipperRequestId = Guid.Parse("34343434-3434-4434-8434-343434343434");
            var shipperId = Guid.Parse("56565656-5656-4656-8656-565656565656");
            var seededAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1, 0, 0, 0), DateTimeKind.Utc);
            var approvedAt = seededAt.AddDays(1);

            builder.Entity<User>().HasData(
                new User
                {
                    Id = adminUserId,
                    FullName = "Seeded Admin",
                    AvatarUrl = "users/55555555-5555-4555-8555-555555555555/avatars/admin.png",
                    PhoneNumber = "0900000000",
                    Status = UserStatus.Active,
                    CreatedAt = seededAt
                },
                new User
                {
                    Id = customerUserId,
                    FullName = "Seeded Customer",
                    AvatarUrl = "users/aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa/avatars/customer.png",
                    PhoneNumber = "0900000001",
                    Status = UserStatus.Active,
                    CreatedAt = seededAt
                },
                new User
                {
                    Id = merchantUserId,
                    FullName = "Seeded Merchant Owner",
                    AvatarUrl = "users/bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb/avatars/merchant.png",
                    PhoneNumber = "0900000002",
                    Status = UserStatus.Active,
                    CreatedAt = seededAt
                },
                new User
                {
                    Id = merchantTwoUserId,
                    FullName = "Lina Tran",
                    AvatarUrl = "users/b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2/avatars/merchant.png",
                    PhoneNumber = "0900000004",
                    Status = UserStatus.Active,
                    CreatedAt = seededAt
                },
                new User
                {
                    Id = merchantThreeUserId,
                    FullName = "Quang Pham",
                    AvatarUrl = "users/b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3/avatars/merchant.png",
                    PhoneNumber = "0900000005",
                    Status = UserStatus.Active,
                    CreatedAt = seededAt
                },
                new User
                {
                    Id = shipperUserId,
                    FullName = "Seeded Shipper",
                    AvatarUrl = "users/99999999-9999-4999-9999-999999999999/avatars/shipper.png",
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
                    Id = merchantTwoAddressId,
                    UserId = merchantTwoUserId,
                    Label = "Store Contact",
                    RecipientName = "Lina Tran",
                    Phone = "0900000004",
                    AddressLine = "15 Pasteur",
                    Ward = "Ben Nghe",
                    District = "District 1",
                    City = "Ho Chi Minh City",
                    Lat = 10.7781m,
                    Lng = 106.6993m,
                    IsDefault = true,
                    CreatedAt = seededAt
                },
                new Address
                {
                    Id = merchantThreeAddressId,
                    UserId = merchantThreeUserId,
                    Label = "Store Contact",
                    RecipientName = "Quang Pham",
                    Phone = "0900000005",
                    AddressLine = "42 Cach Mang Thang 8",
                    Ward = "Vo Thi Sau",
                    District = "District 3",
                    City = "Ho Chi Minh City",
                    Lat = 10.7815m,
                    Lng = 106.6843m,
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

            builder.Entity<MerchantRequest>().HasData(
                new MerchantRequest
                {
                    Id = merchantRequestId,
                    UserId = merchantUserId,
                    StoreName = "Seeded Merchant Store",
                    StoreDescription = "Default merchant store for local development.",
                    BusinessLicense = "BL-SEED-0001",
                    BusinessLicenseUrl = "merchant-requests/bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb/business-license.jpg",
                    TaxId = "TAX-SEED-0001",
                    VerificationStatus = VerificationStatus.Approved,
                    RejectedReason = string.Empty,
                    VerifiedAt = approvedAt,
                    CreatedAt = seededAt,
                    ReviewedBy = adminUserId
                },
                new MerchantRequest
                {
                    Id = merchantTwoRequestId,
                    UserId = merchantTwoUserId,
                    StoreName = "Saigon Fresh Bowls",
                    StoreDescription = "Healthy rice bowls, salads, and fresh juices for busy lunches.",
                    BusinessLicense = "BL-SEED-0002",
                    BusinessLicenseUrl = "merchant-requests/b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2/business-license.jpg",
                    TaxId = "TAX-SEED-0002",
                    VerificationStatus = VerificationStatus.Approved,
                    RejectedReason = string.Empty,
                    VerifiedAt = approvedAt,
                    CreatedAt = seededAt,
                    ReviewedBy = adminUserId
                },
                new MerchantRequest
                {
                    Id = merchantThreeRequestId,
                    UserId = merchantThreeUserId,
                    StoreName = "Banh Mi Corner",
                    StoreDescription = "Vietnamese sandwiches, coffee, and quick breakfast sets.",
                    BusinessLicense = "BL-SEED-0003",
                    BusinessLicenseUrl = "merchant-requests/b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3/business-license.jpg",
                    TaxId = "TAX-SEED-0003",
                    VerificationStatus = VerificationStatus.Approved,
                    RejectedReason = string.Empty,
                    VerifiedAt = approvedAt,
                    CreatedAt = seededAt,
                    ReviewedBy = adminUserId
                }
            );

            builder.Entity<Merchant>().HasData(
                new Merchant
                {
                    Id = merchantId,
                    UserId = merchantUserId,
                    StoreName = "Seeded Merchant Store",
                    StoreDescription = "Default merchant store for local development.",
                    StoreLogoUrl = "merchants/cccccccc-cccc-4ccc-8ccc-cccccccccccc/logo.png",
                    StoreBannerUrl = "merchants/cccccccc-cccc-4ccc-8ccc-cccccccccccc/banner.png",
                    BusinessLicense = "BL-SEED-0001",
                    TaxId = "TAX-SEED-0001",
                    IsOpen = true,
                    OpeningTime = new TimeSpan(8, 0, 0),
                    ClosingTime = new TimeSpan(22, 0, 0),
                    MinOrderAmount = 30000m,
                    AvgPrepTime = 20,
                    Status = MerchantStatus.Approved,
                    CreatedAt = approvedAt
                },
                new Merchant
                {
                    Id = merchantTwoId,
                    UserId = merchantTwoUserId,
                    StoreName = "Saigon Fresh Bowls",
                    StoreDescription = "Healthy rice bowls, salads, and fresh juices for busy lunches.",
                    StoreLogoUrl = "merchants/c2c2c2c2-c2c2-4c2c-8c2c-c2c2c2c2c2c2/logo.png",
                    StoreBannerUrl = "merchants/c2c2c2c2-c2c2-4c2c-8c2c-c2c2c2c2c2c2/banner.png",
                    BusinessLicense = "BL-SEED-0002",
                    TaxId = "TAX-SEED-0002",
                    IsOpen = true,
                    OpeningTime = new TimeSpan(9, 0, 0),
                    ClosingTime = new TimeSpan(21, 30, 0),
                    MinOrderAmount = 45000m,
                    AvgPrepTime = 18,
                    Status = MerchantStatus.Approved,
                    CreatedAt = approvedAt
                },
                new Merchant
                {
                    Id = merchantThreeId,
                    UserId = merchantThreeUserId,
                    StoreName = "Banh Mi Corner",
                    StoreDescription = "Vietnamese sandwiches, coffee, and quick breakfast sets.",
                    StoreLogoUrl = "merchants/c3c3c3c3-c3c3-4c3c-8c3c-c3c3c3c3c3c3/logo.png",
                    StoreBannerUrl = "merchants/c3c3c3c3-c3c3-4c3c-8c3c-c3c3c3c3c3c3/banner.png",
                    BusinessLicense = "BL-SEED-0003",
                    TaxId = "TAX-SEED-0003",
                    IsOpen = true,
                    OpeningTime = new TimeSpan(6, 30, 0),
                    ClosingTime = new TimeSpan(20, 0, 0),
                    MinOrderAmount = 25000m,
                    AvgPrepTime = 12,
                    Status = MerchantStatus.Approved,
                    CreatedAt = approvedAt
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
                    CreatedAt = approvedAt
                },
                new MerchantAddress
                {
                    Id = merchantTwoStoreAddressId,
                    MerchantId = merchantTwoId,
                    AddressLine = "15 Pasteur",
                    Ward = "Ben Nghe",
                    District = "District 1",
                    City = "Ho Chi Minh City",
                    Lat = 10.7781m,
                    Lng = 106.6993m,
                    CreatedAt = approvedAt
                },
                new MerchantAddress
                {
                    Id = merchantThreeStoreAddressId,
                    MerchantId = merchantThreeId,
                    AddressLine = "42 Cach Mang Thang 8",
                    Ward = "Vo Thi Sau",
                    District = "District 3",
                    City = "Ho Chi Minh City",
                    Lat = 10.7815m,
                    Lng = 106.6843m,
                    CreatedAt = approvedAt
                }
            );

            builder.Entity<ShipperRequest>().HasData(
                new ShipperRequest
                {
                    Id = shipperRequestId,
                    UserId = shipperUserId,
                    LicenseNumber = "DL-SEED-0001",
                    LicenseFrontUrl = "shippers/99999999-9999-4999-9999-999999999999/license-front.jpg",
                    LicenseBackUrl = "shippers/99999999-9999-4999-9999-999999999999/license-back.jpg",
                    IdCardFrontUrl = "shippers/99999999-9999-4999-9999-999999999999/id-front.jpg",
                    IdCardBackUrl = "shippers/99999999-9999-4999-9999-999999999999/id-back.jpg",
                    SelfieUrl = "shippers/99999999-9999-4999-9999-999999999999/selfie.jpg",
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
