using Microsoft.EntityFrameworkCore;
using UserService.Entities;
using UserService.Enums;

namespace UserService.Persistences
{
    public class Seeding
    {
        public static void InitializeData(ModelBuilder builder)
        {
            var customerUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var merchantUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var merchantId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
            var customerAddressId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
            var merchantAddressId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
            var merchantStoreAddressId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
            var seededAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1, 0, 0, 0), DateTimeKind.Utc);

            builder.Entity<User>().HasData(
                new User
                {
                    Id = customerUserId,
                    FullName = "Seeded Customer",
                    AvatarUrl = "https://example.com/avatars/customer.png",
                    Status = UserStatus.Active,
                    CreatedAt = seededAt
                },
                new User
                {
                    Id = merchantUserId,
                    FullName = "Seeded Merchant Owner",
                    AvatarUrl = "https://example.com/avatars/merchant.png",
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
        }
    }
}
