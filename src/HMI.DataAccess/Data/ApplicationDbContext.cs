using Microsoft.EntityFrameworkCore;
using HMI.Platform.Core.Models;

namespace HMI.DataAccess.Data;

/// <summary>
/// Application database context for Entity Framework Core.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Vessels in the system.
    /// </summary>
    public DbSet<Vessel> Vessels { get; set; }

    /// <summary>
    /// Engines in the system.
    /// </summary>
    public DbSet<Engine> Engines { get; set; }

    /// <summary>
    /// Sensors in the system.
    /// </summary>
    public DbSet<Sensor> Sensors { get; set; }

    /// <summary>
    /// Alarms in the system.
    /// </summary>
    public DbSet<Alarm> Alarms { get; set; }

    /// <summary>
    /// Message bus events for audit trail.
    /// </summary>
    public DbSet<MessageBusEvent> MessageBusEvents { get; set; }

    /// <summary>
    /// API keys for service-to-service authentication.
    /// </summary>
    public DbSet<ApiKey> ApiKeys { get; set; }

    /// <summary>
    /// Users in the system.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Refresh tokens for JWT authentication.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Vessel entity
        modelBuilder.Entity<Vessel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.HasIndex(e => e.Name);
        });

        // Configure Engine entity
        modelBuilder.Entity<Engine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.VesselId).HasMaxLength(50);
            entity.HasIndex(e => e.VesselId);
            entity.HasIndex(e => e.Name);
        });

        // Configure Sensor entity
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
        });

        // Configure Alarm entity
        modelBuilder.Entity<Alarm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.VesselId).HasMaxLength(50);
            entity.Property(e => e.EngineId).HasMaxLength(50);
            entity.Property(e => e.SensorId).HasMaxLength(50);
            entity.Property(e => e.AcknowledgedBy).HasMaxLength(100);
            entity.Property(e => e.ClearedBy).HasMaxLength(100);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.TriggeredAt);
            entity.HasIndex(e => e.VesselId);
        });

        // Configure MessageBusEvent entity
        modelBuilder.Entity<MessageBusEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.Data).HasColumnType("TEXT");
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Source);
        });

        // Configure ApiKey entity
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.KeyHash).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.OwnerId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OwnerType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Scopes).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(e => e.AllowedIPs).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(e => e.Metadata).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>());
            entity.HasIndex(e => e.KeyHash).IsUnique();
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.ExpiresAt);
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Metadata).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>());
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.TokenHash).HasMaxLength(256).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.JwtId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RevokedReason).HasMaxLength(200);
            entity.Property(e => e.CreatedByIp).HasMaxLength(45);
            entity.Property(e => e.UsedByIp).HasMaxLength(45);
            entity.Property(e => e.RevokedByIp).HasMaxLength(45);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(256);
            entity.Property(e => e.Metadata).HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>());
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.JwtId);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.RevokedAt);
            entity.HasIndex(e => e.UsedAt);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Vessels
        modelBuilder.Entity<Vessel>().HasData(
            new Vessel
            {
                Id = "vessel-001",
                Name = "MV Atlantic Explorer",
                Type = "Container Ship",
                Status = VesselStatus.Online,
                Location = "Port of Rotterdam",
                Length = 300.0,
                Width = 45.0,
                MaxSpeed = 24.5,
                CreatedAt = DateTime.UtcNow
            },
            new Vessel
            {
                Id = "vessel-002",
                Name = "MV Pacific Navigator",
                Type = "Bulk Carrier",
                Status = VesselStatus.Online,
                Location = "Port of Singapore",
                Length = 280.0,
                Width = 42.0,
                MaxSpeed = 22.0,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed Engines
        modelBuilder.Entity<Engine>().HasData(
            new Engine
            {
                Id = "engine-001",
                Name = "Main Engine Port",
                Type = "Diesel",
                Status = EngineStatus.Running,
                VesselId = "vessel-001",
                RPM = 1200,
                MaxRPM = 1800,
                Temperature = 85.5,
                FuelConsumption = 12.5,
                IsRunning = true
            },
            new Engine
            {
                Id = "engine-002",
                Name = "Main Engine Starboard",
                Type = "Diesel",
                Status = EngineStatus.Running,
                VesselId = "vessel-001",
                RPM = 1180,
                MaxRPM = 1800,
                Temperature = 87.2,
                FuelConsumption = 12.8,
                IsRunning = true
            },
            new Engine
            {
                Id = "engine-003",
                Name = "Main Engine",
                Type = "Diesel",
                Status = EngineStatus.Stopped,
                VesselId = "vessel-002",
                RPM = 0,
                MaxRPM = 1600,
                Temperature = 25.0,
                FuelConsumption = 0.0,
                IsRunning = false
            }
        );

        // Seed Sensors
        modelBuilder.Entity<Sensor>().HasData(
            new Sensor
            {
                Id = "sensor-001",
                Name = "Engine Temperature Sensor 1",
                Type = "Temperature",
                Unit = "Celsius",
                Value = 85.5,
                MinValue = 0,
                MaxValue = 120,
                IsActive = true,
                LastUpdated = DateTime.UtcNow
            },
            new Sensor
            {
                Id = "sensor-002",
                Name = "Engine Pressure Sensor 1",
                Type = "Pressure",
                Unit = "Bar",
                Value = 2.5,
                MinValue = 0,
                MaxValue = 10,
                IsActive = true,
                LastUpdated = DateTime.UtcNow
            },
            new Sensor
            {
                Id = "sensor-003",
                Name = "Fuel Flow Sensor 1",
                Type = "Flow",
                Unit = "L/min",
                Value = 12.5,
                MinValue = 0,
                MaxValue = 50,
                IsActive = true,
                LastUpdated = DateTime.UtcNow
            }
        );
    }
}
