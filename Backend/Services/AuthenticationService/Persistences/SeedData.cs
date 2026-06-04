using AuthenticationService.Entities;
using AuthenticationService.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Persistences
{
    public class SeedData
    {
        public static void InitializeData(ModelBuilder builder)
        {
            var samplePasswordHash = "AQAAAAIAAYagAAAAEHwR9ekhFFazFAUin52gHpisNjCdZFdVS0N+lAgSluDA+/uEYfuONXMTsB3L8jrAdA==";
            var adminRoleId = Guid.Parse("44444444-4444-4444-8444-444444444444");
            var adminUserId = Guid.Parse("55555555-5555-4555-8555-555555555555");
            var customerRoleId = Guid.Parse("11111111-1111-4111-8111-111111111111");
            var merchantRoleId = Guid.Parse("22222222-2222-4222-8222-222222222222");
            var shipperRoleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
            var customerUserId = Guid.Parse("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa");
            var merchantUserId = Guid.Parse("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb");
            var merchantTwoUserId = Guid.Parse("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2");
            var merchantThreeUserId = Guid.Parse("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3");
            var shipperUserId = Guid.Parse("99999999-9999-4999-9999-999999999999");
            var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole { Id = customerRoleId, Name = "Customer", NormalizedName = "CUSTOMER", ConcurrencyStamp = "SEED-CUSTOMER-ROLE-CONCURRENCY-STAMP" },
                new ApplicationRole { Id = merchantRoleId, Name = "Merchant", NormalizedName = "MERCHANT", ConcurrencyStamp = "SEED-MERCHANT-ROLE-CONCURRENCY-STAMP" },
                new ApplicationRole { Id = shipperRoleId, Name = "Shipper", NormalizedName = "SHIPPER", ConcurrencyStamp = "SEED-SHIPPER-ROLE-CONCURRENCY-STAMP" },
                new ApplicationRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "SEED-ADMIN-ROLE-CONCURRENCY-STAMP" }
            );

            var adminUser = new ApplicationUser
            {
                Id = adminUserId,
                FullName = "Seeded Admin",
                UserName = "admin@fooddelivery.local",
                NormalizedUserName = "ADMIN@FOODDELIVERY.LOCAL",
                Email = "admin@fooddelivery.local",
                NormalizedEmail = "ADMIN@FOODDELIVERY.LOCAL",
                EmailConfirmed = true,
                SecurityStamp = "SEED-ADMIN-SECURITY-STAMP",
                ConcurrencyStamp = "SEED-ADMIN-CONCURRENCY-STAMP",
                IsOtpVerified = true,
                Status = AuthStatus.Active,
                CreatedAt = seededAt
            };

            adminUser.PasswordHash = samplePasswordHash;

            var customerUser = new ApplicationUser
            {
                Id = customerUserId,
                FullName = "Seeded Customer",
                UserName = "customer@fooddelivery.local",
                NormalizedUserName = "CUSTOMER@FOODDELIVERY.LOCAL",
                Email = "customer@fooddelivery.local",
                NormalizedEmail = "CUSTOMER@FOODDELIVERY.LOCAL",
                EmailConfirmed = true,
                SecurityStamp = "SEED-CUSTOMER-SECURITY-STAMP",
                ConcurrencyStamp = "SEED-CUSTOMER-CONCURRENCY-STAMP",
                IsOtpVerified = true,
                Status = AuthStatus.Active,
                CreatedAt = seededAt,
                PasswordHash = samplePasswordHash
            };

            var merchantUser = new ApplicationUser
            {
                Id = merchantUserId,
                FullName = "Seeded Merchant Owner",
                UserName = "merchant@fooddelivery.local",
                NormalizedUserName = "MERCHANT@FOODDELIVERY.LOCAL",
                Email = "merchant@fooddelivery.local",
                NormalizedEmail = "MERCHANT@FOODDELIVERY.LOCAL",
                EmailConfirmed = true,
                SecurityStamp = "SEED-MERCHANT-SECURITY-STAMP",
                ConcurrencyStamp = "SEED-MERCHANT-CONCURRENCY-STAMP",
                IsOtpVerified = true,
                Status = AuthStatus.Active,
                CreatedAt = seededAt,
                PasswordHash = samplePasswordHash
            };

            var merchantTwoUser = new ApplicationUser
            {
                Id = merchantTwoUserId,
                FullName = "Lina Tran",
                UserName = "lina.tran@fooddelivery.local",
                NormalizedUserName = "LINA.TRAN@FOODDELIVERY.LOCAL",
                Email = "lina.tran@fooddelivery.local",
                NormalizedEmail = "LINA.TRAN@FOODDELIVERY.LOCAL",
                EmailConfirmed = true,
                SecurityStamp = "SEED-MERCHANT-TWO-SECURITY-STAMP",
                ConcurrencyStamp = "SEED-MERCHANT-TWO-CONCURRENCY-STAMP",
                IsOtpVerified = true,
                Status = AuthStatus.Active,
                CreatedAt = seededAt,
                PasswordHash = samplePasswordHash
            };

            var merchantThreeUser = new ApplicationUser
            {
                Id = merchantThreeUserId,
                FullName = "Quang Pham",
                UserName = "quang.pham@fooddelivery.local",
                NormalizedUserName = "QUANG.PHAM@FOODDELIVERY.LOCAL",
                Email = "quang.pham@fooddelivery.local",
                NormalizedEmail = "QUANG.PHAM@FOODDELIVERY.LOCAL",
                EmailConfirmed = true,
                SecurityStamp = "SEED-MERCHANT-THREE-SECURITY-STAMP",
                ConcurrencyStamp = "SEED-MERCHANT-THREE-CONCURRENCY-STAMP",
                IsOtpVerified = true,
                Status = AuthStatus.Active,
                CreatedAt = seededAt,
                PasswordHash = samplePasswordHash
            };

            var shipperUser = new ApplicationUser
            {
                Id = shipperUserId,
                FullName = "Seeded Shipper",
                UserName = "shipper@fooddelivery.local",
                NormalizedUserName = "SHIPPER@FOODDELIVERY.LOCAL",
                Email = "shipper@fooddelivery.local",
                NormalizedEmail = "SHIPPER@FOODDELIVERY.LOCAL",
                EmailConfirmed = true,
                SecurityStamp = "SEED-SHIPPER-SECURITY-STAMP",
                ConcurrencyStamp = "SEED-SHIPPER-CONCURRENCY-STAMP",
                IsOtpVerified = true,
                Status = AuthStatus.Active,
                CreatedAt = seededAt,
                PasswordHash = samplePasswordHash
            };

            builder.Entity<ApplicationUser>().HasData(adminUser, customerUser, merchantUser, merchantTwoUser, merchantThreeUser, shipperUser);

            builder.Entity<IdentityUserRole<Guid>>().HasData(
                new IdentityUserRole<Guid>
                {
                    UserId = adminUserId,
                    RoleId = adminRoleId
                },
                new IdentityUserRole<Guid>
                {
                    UserId = customerUserId,
                    RoleId = customerRoleId
                },
                new IdentityUserRole<Guid>
                {
                    UserId = merchantUserId,
                    RoleId = customerRoleId
                },
                new IdentityUserRole<Guid>
                {
                    UserId = merchantUserId,
                    RoleId = merchantRoleId
                },
                new IdentityUserRole<Guid>
                {
                    UserId = merchantTwoUserId,
                    RoleId = customerRoleId
                },
                new IdentityUserRole<Guid>
                {
                    UserId = merchantTwoUserId,
                    RoleId = merchantRoleId
                },
                new IdentityUserRole<Guid>
                {
                    UserId = merchantThreeUserId,
                    RoleId = customerRoleId
                },
                new IdentityUserRole<Guid>
                {
                    UserId = merchantThreeUserId,
                    RoleId = merchantRoleId
                },
                new IdentityUserRole<Guid>
                {
                    UserId = shipperUserId,
                    RoleId = shipperRoleId
                }
            );
        }
    }
}
