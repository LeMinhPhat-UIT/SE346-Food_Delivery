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
            var adminRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var adminUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Customer", NormalizedName = "CUSTOMER" },
                new ApplicationRole { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Merchant", NormalizedName = "MERCHANT" },
                new ApplicationRole { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Shipper", NormalizedName = "SHIPPER" },
                new ApplicationRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" }
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

            adminUser.PasswordHash = "AQAAAAIAAYagAAAAEHwR9ekhFFazFAUin52gHpisNjCdZFdVS0N+lAgSluDA+/uEYfuONXMTsB3L8jrAdA==";

            builder.Entity<ApplicationUser>().HasData(adminUser);

            builder.Entity<IdentityUserRole<Guid>>().HasData(
                new IdentityUserRole<Guid>
                {
                    UserId = adminUserId,
                    RoleId = adminRoleId
                }
            );
        }
    }
}
