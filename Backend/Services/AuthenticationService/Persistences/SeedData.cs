using AuthenticationService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Persistences
{
    public class SeedData
    {
        public static void InitializeData(ModelBuilder builder)
        {
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Customer", NormalizedName = "CUSTOMER" },
                new ApplicationRole { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Merchant", NormalizedName = "MERCHANT" },
                new ApplicationRole { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Shipper", NormalizedName = "SHIPPER" },
                new ApplicationRole { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Admin", NormalizedName = "ADMIN" }
            );
        }
    }
}
