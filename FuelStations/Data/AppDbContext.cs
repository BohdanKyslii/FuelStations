using FuelStations.Entities;
using Microsoft.EntityFrameworkCore;

namespace FuelStations.Data;

/// <summary>
/// EF Core database context: entry point for all queries and schema configuration.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Station> Stations => Set<Station>();

    public DbSet<StationTerminal> StationTerminals => Set<StationTerminal>();

    public DbSet<StationFuel> StationFuels => Set<StationFuel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Station>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(100).IsRequired();
            entity.Property(s => s.City).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Address).HasMaxLength(250).IsRequired();
            entity.Property(s => s.Descripton).HasMaxLength(500).IsRequired();

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
            // Store enum as readable text ('A95') instead of a number.
            entity.Property(f => f.FuelType).HasConversion<string>().HasMaxLength(20);

            // numeric(10, 2): exact money precision.
            entity.Property(f => f.Price).HasPrecision(10, 2);

            // Business rule: one record per fuel type per station.
            entity.HasIndex(f => new { f.StationId, f.FuelType }).IsUnique();

            // Business rule duplicated at DB level: price must be positive.
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_StationFuels_Price_Positive",
                "\"Price\" > 0"));

            entity.HasOne(f => f.Station)
                .WithMany(s => s.Fuels)
                .HasForeignKey(f => f.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
