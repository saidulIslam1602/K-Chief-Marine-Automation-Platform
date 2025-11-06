using HMI.Platform.Core.Models;

namespace HMI.DataAccess.Interfaces;

/// <summary>
/// Repository interface for Vessel entities.
/// </summary>
public interface IVesselRepository : IRepository<Vessel>
{
    /// <summary>
    /// Gets vessels by status.
    /// </summary>
    Task<IEnumerable<Vessel>> GetByStatusAsync(string status);

    /// <summary>
    /// Gets vessels by type.
    /// </summary>
    Task<IEnumerable<Vessel>> GetByTypeAsync(string type);

    /// <summary>
    /// Gets vessels with their engines.
    /// </summary>
    Task<IEnumerable<Vessel>> GetVesselsWithEnginesAsync();

    /// <summary>
    /// Gets a vessel with its engines by ID.
    /// </summary>
    Task<Vessel?> GetVesselWithEnginesAsync(string vesselId);
}

/// <summary>
/// Repository interface for Engine entities.
/// </summary>
public interface IEngineRepository : IRepository<Engine>
{
    /// <summary>
    /// Gets engines by vessel ID.
    /// </summary>
    Task<IEnumerable<Engine>> GetByVesselIdAsync(string vesselId);

    /// <summary>
    /// Gets running engines.
    /// </summary>
    Task<IEnumerable<Engine>> GetRunningEnginesAsync();

    /// <summary>
    /// Gets engines by status.
    /// </summary>
    Task<IEnumerable<Engine>> GetByStatusAsync(string status);
}

/// <summary>
/// Repository interface for Sensor entities.
/// </summary>
public interface ISensorRepository : IRepository<Sensor>
{
    /// <summary>
    /// Gets sensors by type.
    /// </summary>
    Task<IEnumerable<Sensor>> GetByTypeAsync(string type);

    /// <summary>
    /// Gets active sensors.
    /// </summary>
    Task<IEnumerable<Sensor>> GetActiveSensorsAsync();

    /// <summary>
    /// Gets sensors with values outside normal range.
    /// </summary>
    Task<IEnumerable<Sensor>> GetSensorsOutOfRangeAsync();
}

/// <summary>
/// Repository interface for Alarm entities.
/// </summary>
public interface IAlarmRepository : IRepository<Alarm>
{
    /// <summary>
    /// Gets active alarms.
    /// </summary>
    Task<IEnumerable<Alarm>> GetActiveAlarmsAsync();

    /// <summary>
    /// Gets alarms by severity.
    /// </summary>
    Task<IEnumerable<Alarm>> GetBySeverityAsync(AlarmSeverity severity);

    /// <summary>
    /// Gets alarms by vessel ID.
    /// </summary>
    Task<IEnumerable<Alarm>> GetByVesselIdAsync(string vesselId);

    /// <summary>
    /// Gets alarms by engine ID.
    /// </summary>
    Task<IEnumerable<Alarm>> GetByEngineIdAsync(string engineId);

    /// <summary>
    /// Gets alarms within date range.
    /// </summary>
    Task<IEnumerable<Alarm>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// Repository interface for MessageBusEvent entities.
/// </summary>
public interface IMessageBusEventRepository : IRepository<MessageBusEvent>
{
    /// <summary>
    /// Gets events by type.
    /// </summary>
    Task<IEnumerable<MessageBusEvent>> GetByEventTypeAsync(string eventType);

    /// <summary>
    /// Gets events by source.
    /// </summary>
    Task<IEnumerable<MessageBusEvent>> GetBySourceAsync(string source);

    /// <summary>
    /// Gets events within date range.
    /// </summary>
    Task<IEnumerable<MessageBusEvent>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
