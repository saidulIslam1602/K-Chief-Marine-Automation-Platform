using Microsoft.EntityFrameworkCore;
using HMI.Platform.Core.Models;
using HMI.DataAccess.Data;
using HMI.DataAccess.Interfaces;

namespace HMI.DataAccess.Repositories;

/// <summary>
/// Repository implementation for Vessel entities.
/// </summary>
public class VesselRepository : Repository<Vessel>, IVesselRepository
{
    public VesselRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Vessel>> GetByStatusAsync(string status)
    {
        if (Enum.TryParse<VesselStatus>(status, out var vesselStatus))
        {
            return await _dbSet.Where(v => v.Status == vesselStatus).ToListAsync();
        }
        return new List<Vessel>();
    }

    public async Task<IEnumerable<Vessel>> GetByTypeAsync(string type)
    {
        return await _dbSet.Where(v => v.Type == type).ToListAsync();
    }

    public async Task<IEnumerable<Vessel>> GetVesselsWithEnginesAsync()
    {
        // Note: This would require navigation properties to be set up
        // For now, returning vessels only
        return await _dbSet.ToListAsync();
    }

    public async Task<Vessel?> GetVesselWithEnginesAsync(string vesselId)
    {
        // Note: This would require navigation properties to be set up
        // For now, returning vessel only
        return await _dbSet.FirstOrDefaultAsync(v => v.Id == vesselId);
    }
}

/// <summary>
/// Repository implementation for Engine entities.
/// </summary>
public class EngineRepository : Repository<Engine>, IEngineRepository
{
    public EngineRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Engine>> GetByVesselIdAsync(string vesselId)
    {
        return await _dbSet.Where(e => e.VesselId == vesselId).ToListAsync();
    }

    public async Task<IEnumerable<Engine>> GetRunningEnginesAsync()
    {
        return await _dbSet.Where(e => e.IsRunning).ToListAsync();
    }

    public async Task<IEnumerable<Engine>> GetByStatusAsync(string status)
    {
        if (Enum.TryParse<EngineStatus>(status, out var engineStatus))
        {
            return await _dbSet.Where(e => e.Status == engineStatus).ToListAsync();
        }
        return new List<Engine>();
    }
}

/// <summary>
/// Repository implementation for Sensor entities.
/// </summary>
public class SensorRepository : Repository<Sensor>, ISensorRepository
{
    public SensorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Sensor>> GetByTypeAsync(string type)
    {
        return await _dbSet.Where(s => s.Type == type).ToListAsync();
    }

    public async Task<IEnumerable<Sensor>> GetActiveSensorsAsync()
    {
        return await _dbSet.Where(s => s.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<Sensor>> GetSensorsOutOfRangeAsync()
    {
        return await _dbSet.Where(s => s.Value < s.MinValue || s.Value > s.MaxValue).ToListAsync();
    }
}

/// <summary>
/// Repository implementation for Alarm entities.
/// </summary>
public class AlarmRepository : Repository<Alarm>, IAlarmRepository
{
    public AlarmRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Alarm>> GetActiveAlarmsAsync()
    {
        return await _dbSet.Where(a => a.Status == AlarmStatus.Active).ToListAsync();
    }

    public async Task<IEnumerable<Alarm>> GetBySeverityAsync(AlarmSeverity severity)
    {
        return await _dbSet.Where(a => a.Severity == severity).ToListAsync();
    }

    public async Task<IEnumerable<Alarm>> GetByVesselIdAsync(string vesselId)
    {
        return await _dbSet.Where(a => a.VesselId == vesselId).ToListAsync();
    }

    public async Task<IEnumerable<Alarm>> GetByEngineIdAsync(string engineId)
    {
        return await _dbSet.Where(a => a.EngineId == engineId).ToListAsync();
    }

    public async Task<IEnumerable<Alarm>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.Where(a => a.TriggeredAt >= startDate && a.TriggeredAt <= endDate).ToListAsync();
    }
}

/// <summary>
/// Repository implementation for MessageBusEvent entities.
/// </summary>
public class MessageBusEventRepository : Repository<MessageBusEvent>, IMessageBusEventRepository
{
    public MessageBusEventRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MessageBusEvent>> GetByEventTypeAsync(string eventType)
    {
        return await _dbSet.Where(e => e.EventType == eventType).ToListAsync();
    }

    public async Task<IEnumerable<MessageBusEvent>> GetBySourceAsync(string source)
    {
        return await _dbSet.Where(e => e.Source == source).ToListAsync();
    }

    public async Task<IEnumerable<MessageBusEvent>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet.Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate).ToListAsync();
    }
}
