using AddressService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AddressService.Persistences
{
    public class AddressDbContext : DbContext
    {
        public AddressDbContext(DbContextOptions<AddressDbContext> options) : base(options)
        {
        }

        public DbSet<AdministrativeRegion> AdministrativeRegions => Set<AdministrativeRegion>();
        public DbSet<AdministrativeUnit> AdministrativeUnits => Set<AdministrativeUnit>();
        public DbSet<Province> Provinces => Set<Province>();
        public DbSet<Ward> Wards => Set<Ward>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AdministrativeRegion>(entity =>
            {
                entity.ToTable("administrative_regions");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
                entity.Property(e => e.NameEn).HasColumnName("name_en").HasMaxLength(255).IsRequired();
                entity.Property(e => e.CodeName).HasColumnName("code_name").HasMaxLength(255);
                entity.Property(e => e.CodeNameEn).HasColumnName("code_name_en").HasMaxLength(255);
            });

            modelBuilder.Entity<AdministrativeUnit>(entity =>
            {
                entity.ToTable("administrative_units");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255);
                entity.Property(e => e.FullNameEn).HasColumnName("full_name_en").HasMaxLength(255);
                entity.Property(e => e.ShortName).HasColumnName("short_name").HasMaxLength(255);
                entity.Property(e => e.ShortNameEn).HasColumnName("short_name_en").HasMaxLength(255);
                entity.Property(e => e.CodeName).HasColumnName("code_name").HasMaxLength(255);
                entity.Property(e => e.CodeNameEn).HasColumnName("code_name_en").HasMaxLength(255);
            });

            modelBuilder.Entity<Province>(entity =>
            {
                entity.ToTable("provinces");
                entity.HasKey(e => e.Code);

                entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
                entity.Property(e => e.NameEn).HasColumnName("name_en").HasMaxLength(255);
                entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
                entity.Property(e => e.FullNameEn).HasColumnName("full_name_en").HasMaxLength(255);
                entity.Property(e => e.CodeName).HasColumnName("code_name").HasMaxLength(255);
                entity.Property(e => e.AdministrativeUnitId).HasColumnName("administrative_unit_id");

                entity.HasIndex(e => e.AdministrativeUnitId).HasDatabaseName("idx_provinces_unit");

                entity.HasOne(e => e.AdministrativeUnit)
                    .WithMany(e => e.Provinces)
                    .HasForeignKey(e => e.AdministrativeUnitId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Ward>(entity =>
            {
                entity.ToTable("wards");
                entity.HasKey(e => e.Code);

                entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
                entity.Property(e => e.NameEn).HasColumnName("name_en").HasMaxLength(255);
                entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255);
                entity.Property(e => e.FullNameEn).HasColumnName("full_name_en").HasMaxLength(255);
                entity.Property(e => e.CodeName).HasColumnName("code_name").HasMaxLength(255);
                entity.Property(e => e.ProvinceCode).HasColumnName("province_code").HasMaxLength(20);
                entity.Property(e => e.AdministrativeUnitId).HasColumnName("administrative_unit_id");

                entity.HasIndex(e => e.ProvinceCode).HasDatabaseName("idx_wards_province");
                entity.HasIndex(e => e.AdministrativeUnitId).HasDatabaseName("idx_wards_unit");

                entity.HasOne(e => e.Province)
                    .WithMany(e => e.Wards)
                    .HasForeignKey(e => e.ProvinceCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AdministrativeUnit)
                    .WithMany(e => e.Wards)
                    .HasForeignKey(e => e.AdministrativeUnitId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
