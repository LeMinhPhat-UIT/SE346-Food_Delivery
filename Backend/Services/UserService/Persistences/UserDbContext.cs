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
                    .HasForeignKey<Shipper>(u => u.UserId);
                entity
                    .HasOne(u => u.Merchant)
                    .WithOne(s => s.User)
                    .HasForeignKey<Merchant>(u => u.UserId);
                entity
                    .HasMany(u => u.Addresses)
                    .WithOne(a => a.User)
                    .HasForeignKey(a => a.UserId);
            });

            modelBuilder.Entity<Shipper>(entity =>
            {
                entity.HasKey(s => s.Id);
            });

            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity
                    .HasMany(m => m.Addresses)
                    .WithOne(a => a.Merchant)
                    .HasForeignKey(a => a.MerchantId);
            });

            modelBuilder.Entity<ShipperRequest>(entity =>
            {
                entity.HasKey(mr => mr.Id);
                entity
                    .HasOne(mr => mr.User)
                    .WithMany(u => u.ShipperRequests)
                    .HasForeignKey(mr => mr.UserId);

                entity
                    .HasOne(mr => mr.ReviewedUser)
                    .WithMany()
                    .HasForeignKey(mr => mr.ReviewedBy);
            });

            modelBuilder.Entity<MerchantRequest>(entity =>
            {
                entity.HasKey(mr => mr.Id);
                entity
                    .HasOne(mr => mr.User)
                    .WithMany(u => u.MerchantRequests)
                    .HasForeignKey(mr => mr.UserId);

                entity
                    .HasOne(mr => mr.ReviewedUser)
                    .WithMany()
                    .HasForeignKey(mr => mr.ReviewedBy);
            });
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Merchant> Merchants { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<MerchantAddress> MerchantAddresses { get; set; }
        public DbSet<ShipperRequest> ShipperEkycs { get; set; }
        public DbSet<MerchantRequest> MerchantRequests { get; set; }
        public DbSet<ShipperRequest> ShipperRequests { get; set; }
    }
}
