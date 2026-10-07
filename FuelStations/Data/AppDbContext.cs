using FuelStations.Entities;
using Microsoft.EntityFrameworkCore;

namespace FuelStations.Data;

/// <summary>
/// EF Core database context: entry point for all queries and schema configuration.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<FuelType> FuelTypes => Set<FuelType>();

    public DbSet<Station> Stations => Set<Station>();

    public DbSet<StationTerminal> StationTerminals => Set<StationTerminal>();

    public DbSet<StationFuel> StationFuels => Set<StationFuel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FuelType>(entity =>
        {
            // Business code is the natural primary key.
            entity.HasKey(f => f.Code);
            entity.Property(f => f.Code).HasMaxLength(20);
            entity.Property(f => f.Name).HasMaxLength(50).IsRequired();

            entity.HasIndex(f => f.Name).IsUnique();

            // Code format: 2-20 upper-case Latin letters, digits or "_".
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_FuelTypes_Code_Format",
                "\"Code\" ~ '^[A-Z0-9_]{2,20}$'"));
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(100).IsRequired();
            entity.Property(s => s.City).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Address).HasMaxLength(250).IsRequired();

            // Station IDs are bound to the station name, so names must be unique.
            entity.HasIndex(s => s.Name).IsUnique();

            // Speeds up filtering by city.
            entity.HasIndex(s => s.City);
        });

        modelBuilder.Entity<StationTerminal>(entity =>
        {
            // Business station ID is the natural primary key, assigned manually.
            entity.HasKey(t => t.Number);
            entity.Property(t => t.Number).ValueGeneratedNever();

            entity.Property(t => t.Description).HasMaxLength(100);

            entity.ToTable(table => table.HasCheckConstraint(
                "CK_StationTerminals_Number_Range",
                "\"Number\" BETWEEN 1000 AND 9999"));

            entity.HasOne(t => t.Station)
                .WithMany(s => s.Terminals)
                .HasForeignKey(t => t.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StationFuel>(entity =>
        {
            entity.Property(f => f.FuelTypeCode).HasMaxLength(20).IsRequired();

            // numeric(10, 2): exact money precision.
            entity.Property(f => f.Price).HasPrecision(10, 2);

            // Business rule: one record per fuel type per station.
            entity.HasIndex(f => new { f.StationId, f.FuelTypeCode }).IsUnique();

            // Business rule duplicated at DB level: price must be positive.
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_StationFuels_Price_Positive",
                "\"Price\" > 0"));

            entity.HasOne(f => f.Station)
                .WithMany(s => s.Fuels)
                .HasForeignKey(f => f.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            // A fuel type used at any station can't be deleted, only deactivated.
            entity.HasOne(f => f.FuelType)
                .WithMany(t => t.StationFuels)
                .HasForeignKey(f => f.FuelTypeCode)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
