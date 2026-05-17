using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DeliveryService.Persistences
{
    public class DeliveryDbContextFactory : IDesignTimeDbContextFactory<DeliveryDbContext>
    {
        public DeliveryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DeliveryDbContext>();

            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DeliveryDbConnectionString")
                ?? "Host=localhost;Port=5435;Database=delivery_db;Username=delivery_service;Password=delivery_service_password";

            optionsBuilder.UseNpgsql(connectionString);

            return new DeliveryDbContext(optionsBuilder.Options);
        }
    }
}