using DeliveryService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Persistences
{
    public class DeliveryDbContext : DbContext
    {
        public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeliveryTracking>(entity =>
            {
                entity.HasKey(dt => dt.Id);
            });

            modelBuilder.Entity<Incident>(entity =>
            {
                entity.HasKey(i => i.Id);
            });

            modelBuilder.Entity<ShipperAssignment>(entity =>
            {
                entity.HasKey(sa => sa.Id);
            });

            modelBuilder.Entity<ShipperAvailability>(entity =>
            {
                entity.HasKey(sa => sa.Id);
            });
        }

        public DbSet<DeliveryTracking> DeliveryTrackings { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<ShipperAssignment> ShipperAssignments { get; set; }
        public DbSet<ShipperAvailability> ShipperAvailabilities { get; set; }
    }
}
