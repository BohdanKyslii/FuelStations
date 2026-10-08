using FuelStations.Entities;
using Microsoft.EntityFrameworkCore;

namespace FuelStations.Data;

/// <summary>
/// Контекст БД EF Core: точка входу для всіх запитів і налаштування схеми.
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
            // Бізнес-код — природний первинний ключ.
            // Business code is the natural primary key.
            entity.HasKey(f => f.Code);
            entity.Property(f => f.Code).HasMaxLength(20);
            entity.Property(f => f.Name).HasMaxLength(50).IsRequired();

            entity.HasIndex(f => f.Name).IsUnique();

            // Формат коду: 2–20 великих латинських літер, цифр або "_".
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
            entity.Property(s => s.Description).HasMaxLength(500);

            // Назва АЗС унікальна: за нею ID АЗС прив'язані до конкретної заправки.
            // Унікальність самих ID АЗС гарантує первинний ключ StationTerminals.Number.
            // Station name is unique: it binds station IDs to a specific station.
            // Station IDs themselves are unique as the StationTerminals.Number primary key.
            entity.HasIndex(s => s.Name).IsUnique();

            // Пришвидшує фільтрацію за містом.
            // Speeds up filtering by city.
            entity.HasIndex(s => s.City);
        });

        modelBuilder.Entity<StationTerminal>(entity =>
        {
            // ID АЗС — природний первинний ключ: унікальний у всій БД і задається вручну.
            // Один ID не може належати двом АЗС одночасно.
            // Station ID is the natural primary key: unique in the DB, assigned manually.
            // One ID can't belong to two stations at the same time.
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

            // numeric(10, 2): точне зберігання грошових сум.
            // numeric(10, 2): exact money precision.
            entity.Property(f => f.Price).HasPrecision(10, 2);

            // Бізнес-правило: один запис на кожен вид пального на АЗС.
            // Business rule: one record per fuel type per station.
            entity.HasIndex(f => new { f.StationId, f.FuelTypeCode }).IsUnique();

            // Бізнес-правило продубльоване в БД: ціна має бути додатною.
            // Business rule duplicated at DB level: price must be positive.
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_StationFuels_Price_Positive",
                "\"Price\" > 0"));

            entity.HasOne(f => f.Station)
                .WithMany(s => s.Fuels)
                .HasForeignKey(f => f.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Вид пального, що продається на АЗС, не видаляється — лише деактивується.
            // A fuel type used at any station can't be deleted, only deactivated.
            entity.HasOne(f => f.FuelType)
                .WithMany(t => t.StationFuels)
                .HasForeignKey(f => f.FuelTypeCode)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
