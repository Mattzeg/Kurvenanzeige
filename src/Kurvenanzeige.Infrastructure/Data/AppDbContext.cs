using Kurvenanzeige.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kurvenanzeige.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AnalogReading> AnalogReadings => Set<AnalogReading>();
    public DbSet<DigitalReading> DigitalReadings => Set<DigitalReading>();
    public DbSet<DataBlockReading> DataBlockReadings => Set<DataBlockReading>();
    public DbSet<StringReading> StringReadings => Set<StringReading>();
    public DbSet<DataPointConfiguration> DataPointConfigurations => Set<DataPointConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AnalogReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.TagName, e.Timestamp });
            entity.Property(e => e.TagName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<DigitalReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.TagName, e.Timestamp });
            entity.Property(e => e.TagName).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<DataBlockReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.TagName, e.Timestamp });
            entity.Property(e => e.TagName).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<StringReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.TagName, e.Timestamp });
            entity.Property(e => e.TagName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).HasMaxLength(255);
        });

        modelBuilder.Entity<DataPointConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TagName).IsUnique();
            entity.Property(e => e.TagName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DataType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        await Database.MigrateAsync();

        await Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
        await Database.ExecuteSqlRawAsync("PRAGMA synchronous=NORMAL;");
        await Database.ExecuteSqlRawAsync("PRAGMA cache_size=-64000;");

        await SeedDataAsync();
    }

    private async Task SeedDataAsync()
    {
        if (await DataPointConfigurations.AnyAsync())
            return;

        var defaultDataPoints = new[]
        {
            new DataPointConfiguration
            {
                TagName = "Temperature_Reactor1",
                DisplayName = "Reactor 1 Temperature",
                DataType = "Analog",
                DbNumber = 1,
                Offset = 0,
                Unit = "°C",
                MinValue = 0,
                MaxValue = 200,
                IsEnabled = true,
                PollingInterval = 5000,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new DataPointConfiguration
            {
                TagName = "Pressure_Line1",
                DisplayName = "Line 1 Pressure",
                DataType = "Analog",
                DbNumber = 1,
                Offset = 4,
                Unit = "bar",
                MinValue = 0,
                MaxValue = 10,
                IsEnabled = true,
                PollingInterval = 5000,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new DataPointConfiguration
            {
                TagName = "Pump_Running",
                DisplayName = "Main Pump Status",
                DataType = "Digital",
                DbNumber = 2,
                Offset = 0,
                Bit = 0,
                IsEnabled = true,
                PollingInterval = 5000,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        };

        await DataPointConfigurations.AddRangeAsync(defaultDataPoints);
        await SaveChangesAsync();
    }
}
