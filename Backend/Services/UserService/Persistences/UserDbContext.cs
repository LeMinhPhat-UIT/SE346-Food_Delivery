using Microsoft.EntityFrameworkCore;
using UserService.Entities;

namespace UserService.Persistences
{
    public class UserDbContext : DbContext
    {

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity
                    .HasOne(u => u.Shipper)
                    .WithOne(s => s.User)
                    .HasForeignKey<User>(u => u.ShipperId);
                entity
                    .HasOne(u => u.Merchant)
                    .WithOne(s => s.User)
                    .HasForeignKey<User>(u => u.MerchantId);
                entity
                    .HasMany(u => u.Addresses)
                    .WithOne(a => a.User)
                    .HasForeignKey(a => a.UserId);
            });

            modelBuilder.Entity<Shipper>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity
                    .HasOne(s => s.Ekyc)
                    .WithOne(e => e.Shipper)
                    .HasForeignKey<Shipper>(s => s.EkycId);
            });

            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity
                    .HasMany(m => m.Addresses)
                    .WithOne(a => a.Merchant)
                    .HasForeignKey(a => a.MerchantId);
            });
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Merchant> Merchants { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<MerchantAddress> MerchantAddresses { get; set; }
        public DbSet<ShipperEkyc> ShipperEkycs { get; set; }
    }
}
