using Bogus;
using KChief.Platform.Core.Models;

namespace KChief.Platform.Tests.TestHelpers.DataGenerators;

/// <summary>
/// Extensions for generating test data using Bogus.
/// </summary>
public static class FakerExtensions
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Generates a random vessel.
    /// </summary>
    public static Vessel GenerateVessel(this Faker faker, string? id = null)
    {
        return new Vessel
        {
            Id = id ?? $"vessel-{faker.Random.Int(1, 999):D3}",
            Name = faker.Company.CompanyName() + " Vessel",
            IMONumber = $"IMO{faker.Random.Int(1000000, 9999999)}",
            CallSign = faker.Random.AlphaNumeric(4).ToUpper(),
            Type = faker.PickRandom<VesselType>(),
            Length = faker.Random.Double(50, 400),
            Width = faker.Random.Double(10, 60),
            Draft = faker.Random.Double(3, 20),
            GrossTonnage = faker.Random.Double(1000, 50000),
            Flag = faker.Address.CountryCode(),
            BuiltDate = faker.Date.Past(30),
            Owner = faker.Company.CompanyName(),
            Status = faker.PickRandom<VesselStatus>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generates a random engine.
    /// </summary>
    public static Engine GenerateEngine(this Faker faker, string? vesselId = null, string? id = null)
    {
        return new Engine
        {
            Id = id ?? $"engine-{faker.Random.Int(1, 999):D3}",
            VesselId = vesselId ?? $"vessel-{faker.Random.Int(1, 999):D3}",
            Name = faker.PickRandom("Main Engine", "Auxiliary Engine", "Generator Engine"),
            Type = faker.PickRandom<EngineType>(),
            MaxRpm = faker.Random.Double(500, 2000),
            CurrentRpm = faker.Random.Double(0, 1000),
            Status = faker.PickRandom<EngineStatus>(),
            Temperature = faker.Random.Double(20, 100),
            Pressure = faker.Random.Double(0.5, 2.0),
            Manufacturer = faker.Company.CompanyName(),
            Model = faker.Random.AlphaNumeric(10),
            InstalledDate = faker.Date.Past(10),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generates a random alarm.
    /// </summary>
    public static Alarm GenerateAlarm(this Faker faker, string? vesselId = null, string? id = null)
    {
        return new Alarm
        {
            Id = id ?? $"alarm-{faker.Random.Int(1, 999):D3}",
            Title = faker.Lorem.Sentence(),
            Description = faker.Lorem.Paragraph(),
            Severity = faker.PickRandom<AlarmSeverity>(),
            Status = faker.PickRandom<AlarmStatus>(),
            VesselId = vesselId ?? $"vessel-{faker.Random.Int(1, 999):D3}",
            EngineId = faker.Random.Bool(0.5f) ? $"engine-{faker.Random.Int(1, 999):D3}" : null,
            SensorId = faker.Random.Bool(0.3f) ? $"sensor-{faker.Random.Int(1, 999):D3}" : null,
            TriggeredAt = faker.Date.Recent(),
            AcknowledgedAt = faker.Random.Bool(0.3f) ? faker.Date.Recent() : null,
            AcknowledgedBy = faker.Random.Bool(0.3f) ? faker.Person.UserName : null,
            ClearedAt = faker.Random.Bool(0.2f) ? faker.Date.Recent() : null,
            ClearedBy = faker.Random.Bool(0.2f) ? faker.Person.UserName : null
        };
    }

    /// <summary>
    /// Generates multiple vessels.
    /// </summary>
    public static IEnumerable<Vessel> GenerateVessels(this Faker faker, int count)
    {
        return Enumerable.Range(0, count).Select(_ => faker.GenerateVessel());
    }

    /// <summary>
    /// Generates multiple engines.
    /// </summary>
    public static IEnumerable<Engine> GenerateEngines(this Faker faker, int count, string? vesselId = null)
    {
        return Enumerable.Range(0, count).Select(_ => faker.GenerateEngine(vesselId));
    }

    /// <summary>
    /// Generates multiple alarms.
    /// </summary>
    public static IEnumerable<Alarm> GenerateAlarms(this Faker faker, int count, string? vesselId = null)
    {
        return Enumerable.Range(0, count).Select(_ => faker.GenerateAlarm(vesselId));
    }
}

