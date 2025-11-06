using KChief.Platform.Core.Models;

namespace KChief.Platform.Tests.TestHelpers.Builders;

/// <summary>
/// Builder for creating test engine instances.
/// </summary>
public class EngineBuilder
{
    private string _id = "engine-001";
    private string _vesselId = "vessel-001";
    private string _name = "Main Engine";
    private EngineType _type = EngineType.Diesel;
    private double _maxRpm = 1000.0;
    private double _currentRpm = 0.0;
    private EngineStatus _status = EngineStatus.Stopped;
    private double _temperature = 20.0;
    private double _pressure = 1.0;
    private string? _manufacturer = "Test Manufacturer";
    private string? _model = "Test Model";
    private DateTime _installedDate = DateTime.UtcNow.AddYears(-5);

    public EngineBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public EngineBuilder WithVesselId(string vesselId)
    {
        _vesselId = vesselId;
        return this;
    }

    public EngineBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public EngineBuilder WithType(EngineType type)
    {
        _type = type;
        return this;
    }

    public EngineBuilder WithMaxRpm(double maxRpm)
    {
        _maxRpm = maxRpm;
        return this;
    }

    public EngineBuilder WithCurrentRpm(double currentRpm)
    {
        _currentRpm = currentRpm;
        return this;
    }

    public EngineBuilder WithStatus(EngineStatus status)
    {
        _status = status;
        return this;
    }

    public EngineBuilder WithTemperature(double temperature)
    {
        _temperature = temperature;
        return this;
    }

    public EngineBuilder WithPressure(double pressure)
    {
        _pressure = pressure;
        return this;
    }

    public EngineBuilder WithManufacturer(string manufacturer)
    {
        _manufacturer = manufacturer;
        return this;
    }

    public EngineBuilder WithModel(string model)
    {
        _model = model;
        return this;
    }

    public EngineBuilder WithInstalledDate(DateTime installedDate)
    {
        _installedDate = installedDate;
        return this;
    }

    public EngineBuilder AsDiesel()
    {
        _type = EngineType.Diesel;
        return this;
    }

    public EngineBuilder AsGasTurbine()
    {
        _type = EngineType.GasTurbine;
        return this;
    }

    public EngineBuilder Running()
    {
        _status = EngineStatus.Running;
        _currentRpm = _maxRpm * 0.8;
        return this;
    }

    public EngineBuilder Stopped()
    {
        _status = EngineStatus.Stopped;
        _currentRpm = 0.0;
        return this;
    }

    public EngineBuilder Overheated()
    {
        _status = EngineStatus.Overheated;
        _temperature = 120.0;
        return this;
    }

    public Engine Build()
    {
        return new Engine
        {
            Id = _id,
            VesselId = _vesselId,
            Name = _name,
            Type = _type,
            MaxRpm = _maxRpm,
            CurrentRpm = _currentRpm,
            Status = _status,
            Temperature = _temperature,
            Pressure = _pressure,
            Manufacturer = _manufacturer,
            Model = _model,
            InstalledDate = _installedDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static EngineBuilder Create() => new();

    public static EngineBuilder CreateDefault() => new()
        .WithId("engine-001")
        .WithVesselId("vessel-001")
        .WithName("Main Engine")
        .AsDiesel()
        .Stopped();
}

