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
                entity.Property(dt => dt.Status).HasConversion<string>();
            });

            modelBuilder.Entity<Incident>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Type).HasConversion<string>();
                entity.Property(i => i.Status).HasConversion<string>();
            });

            modelBuilder.Entity<ShipperAssignment>(entity =>
            {
                entity.HasKey(sa => sa.Id);
                entity.Property(sa => sa.Status).HasConversion<string>();
                entity.HasIndex(sa => sa.Status);
                entity.HasIndex(sa => new { sa.OrderId, sa.ShipperId }).IsUnique();
                entity.HasIndex(sa => sa.OrderId)
                    .IsUnique()
                    .HasFilter("\"Status\" = 'Accepted'");
            });

            modelBuilder.Entity<ShipperLocationHistory>(entity =>
            {
                entity.HasKey(sl => sl.Id);
                entity.HasIndex(sl => new { sl.OrderId, sl.RecordedAt });
                entity.HasIndex(sl => new { sl.ShipperId, sl.RecordedAt });
            });

            modelBuilder.Entity<ShipperAvailability>(entity =>
            {
                entity.HasKey(sa => sa.Id);
                entity.Property(sa => sa.Status).HasConversion<string>();
                entity.HasIndex(sa => sa.Status);
                entity.HasIndex(sa => sa.CurrentAssignmentId);
                entity.HasIndex(sa => sa.CurrentOfferedAssignmentId);
            });

            modelBuilder.Entity<DeliveryFeePolicy>(entity =>
            {
                entity.HasKey(policy => policy.Id);
                entity.Property(policy => policy.Name)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(policy => policy.BaseFee).HasPrecision(18, 2);
                entity.Property(policy => policy.MinFee).HasPrecision(18, 2);
                entity.Property(policy => policy.MaxFee).HasPrecision(18, 2);
                entity.Property(policy => policy.SmallOrderThreshold).HasPrecision(18, 2);
                entity.Property(policy => policy.SmallOrderSurcharge).HasPrecision(18, 2);
                entity.Property(policy => policy.RushHourSurcharge).HasPrecision(18, 2);
                entity.HasIndex(policy => policy.IsActive);
            });

            modelBuilder.Entity<DeliveryFeeDistanceTier>(entity =>
            {
                entity.HasKey(tier => tier.Id);
                entity.Property(tier => tier.FromKm).HasPrecision(10, 2);
                entity.Property(tier => tier.ToKm).HasPrecision(10, 2);
                entity.Property(tier => tier.FeePerKm).HasPrecision(18, 2);
                entity.HasIndex(tier => tier.PolicyId);
                entity.HasOne(tier => tier.Policy)
                    .WithMany(policy => policy.DistanceTiers)
                    .HasForeignKey(tier => tier.PolicyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DeliveryFeeQuote>(entity =>
            {
                entity.HasKey(quote => quote.Id);
                entity.Property(quote => quote.PickupLat).HasPrecision(10, 7);
                entity.Property(quote => quote.PickupLng).HasPrecision(10, 7);
                entity.Property(quote => quote.DropoffLat).HasPrecision(10, 7);
                entity.Property(quote => quote.DropoffLng).HasPrecision(10, 7);
                entity.Property(quote => quote.DistanceKm).HasPrecision(10, 2);
                entity.Property(quote => quote.Subtotal).HasPrecision(18, 2);
                entity.Property(quote => quote.DeliveryFee).HasPrecision(18, 2);
                entity.Property(quote => quote.Currency).HasMaxLength(10).IsRequired();
                entity.HasIndex(quote => quote.OrderId);
            });

            modelBuilder.Entity<DeliveryFeeQuoteDetail>(entity =>
            {
                entity.HasKey(detail => detail.Id);
                entity.Property(detail => detail.PolicyName).HasMaxLength(100).IsRequired();
                entity.Property(detail => detail.BaseFee).HasPrecision(18, 2);
                entity.Property(detail => detail.DistanceFee).HasPrecision(18, 2);
                entity.Property(detail => detail.SmallOrderSurcharge).HasPrecision(18, 2);
                entity.Property(detail => detail.RushHourSurcharge).HasPrecision(18, 2);
                entity.Property(detail => detail.RawFee).HasPrecision(18, 2);
                entity.Property(detail => detail.FinalFee).HasPrecision(18, 2);
                entity.HasIndex(detail => detail.PolicyId);
                entity.HasIndex(detail => detail.QuoteId);
                entity.HasOne(detail => detail.Quote)
                    .WithMany(quote => quote.Details)
                    .HasForeignKey(detail => detail.QuoteId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(detail => detail.Policy)
                    .WithMany(policy => policy.QuoteDetails)
                    .HasForeignKey(detail => detail.PolicyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            SeedData.InitializeData(modelBuilder);
        }

        public DbSet<DeliveryTracking> DeliveryTrackings { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<ShipperAssignment> ShipperAssignments { get; set; }
        public DbSet<ShipperLocationHistory> ShipperLocationHistories { get; set; }
        public DbSet<ShipperAvailability> ShipperAvailabilities { get; set; }
        public DbSet<DeliveryFeePolicy> DeliveryFeePolicies { get; set; }
        public DbSet<DeliveryFeeDistanceTier> DeliveryFeeDistanceTiers { get; set; }
        public DbSet<DeliveryFeeQuote> DeliveryFeeQuotes { get; set; }
        public DbSet<DeliveryFeeQuoteDetail> DeliveryFeeQuoteDetails { get; set; }
    }
}
