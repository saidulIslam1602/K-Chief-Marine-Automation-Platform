namespace KChief.Platform.Core.Models;

/// <summary>
/// Represents a marine engine in the vessel control system.
/// </summary>
public class Engine
{
    /// <summary>
    /// Unique identifier for the engine.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name or designation of the engine (e.g., "Main Engine 1", "Auxiliary Engine 2").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Engine type (e.g., "Diesel", "Gas Turbine", "Electric").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Current operational status of the engine.
    /// </summary>
    public EngineStatus Status { get; set; } = EngineStatus.Stopped;

    /// <summary>
    /// ID of the vessel this engine belongs to.
    /// </summary>
    public string VesselId { get; set; } = string.Empty;

    /// <summary>
    /// Current RPM (Revolutions Per Minute).
    /// </summary>
    public int RPM { get; set; }

    /// <summary>
    /// Maximum rated RPM for this engine.
    /// </summary>
    public int MaxRPM { get; set; } = 1000;

    /// <summary>
    /// Indicates if the engine is currently running.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Engine temperature in Celsius.
    /// </summary>
    public double Temperature { get; set; }

    /// <summary>
    /// Oil pressure in bar.
    /// </summary>
    public double OilPressure { get; set; }

    /// <summary>
    /// Fuel consumption rate in liters per hour.
    /// </summary>
    public double FuelConsumption { get; set; }

    /// <summary>
    /// Timestamp when the engine data was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Operational status of an engine.
/// </summary>
public enum EngineStatus
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Fault,
    Maintenance
}

