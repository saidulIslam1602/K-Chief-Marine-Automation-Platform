using KChief.Platform.Core.Interfaces;
using KChief.Platform.Core.Models;
using KChief.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;

namespace KChief.VesselControl.Services;

/// <summary>
/// Database-backed implementation of vessel control service using repository pattern.
/// This demonstrates how to use Entity Framework with the repository pattern.
/// </summary>
public class DatabaseVesselControlService : IVesselControlService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DatabaseVesselControlService> _logger;

    public DatabaseVesselControlService(IUnitOfWork unitOfWork, ILogger<DatabaseVesselControlService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Vessel>> GetAllVesselsAsync()
    {
        _logger.LogInformation("Retrieving all vessels from database");
        return await _unitOfWork.Vessels.GetAllAsync();
    }

    public async Task<Vessel?> GetVesselByIdAsync(string vesselId)
    {
        _logger.LogInformation("Retrieving vessel {VesselId} from database", vesselId);
        return await _unitOfWork.Vessels.GetByIdAsync(vesselId);
    }

    public async Task<IEnumerable<Engine>> GetVesselEnginesAsync(string vesselId)
    {
        _logger.LogInformation("Retrieving engines for vessel {VesselId} from database", vesselId);
        return await _unitOfWork.Engines.GetByVesselIdAsync(vesselId);
    }

    public async Task<Engine?> GetEngineByIdAsync(string vesselId, string engineId)
    {
        _logger.LogInformation("Retrieving engine {EngineId} for vessel {VesselId} from database", engineId, vesselId);
        var engine = await _unitOfWork.Engines.GetByIdAsync(engineId);
        
        // Verify the engine belongs to the specified vessel
        if (engine?.VesselId != vesselId)
        {
            return null;
        }
        
        return engine;
    }

    public async Task<bool> StartEngineAsync(string vesselId, string engineId)
    {
        _logger.LogInformation("Starting engine {EngineId} for vessel {VesselId}", engineId, vesselId);
        
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var engine = await GetEngineByIdAsync(vesselId, engineId);
            if (engine == null)
            {
                _logger.LogWarning("Engine {EngineId} not found for vessel {VesselId}", engineId, vesselId);
                return false;
            }

            if (engine.Status == EngineStatus.Running)
            {
                _logger.LogInformation("Engine {EngineId} is already running", engineId);
                return true;
            }

            // Update engine status
            engine.Status = EngineStatus.Running;
            engine.IsRunning = true;
            engine.RPM = 800; // Default startup RPM
            engine.LastUpdated = DateTime.UtcNow;
            
            _unitOfWork.Engines.Update(engine);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation("Engine {EngineId} started successfully", engineId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start engine {EngineId} for vessel {VesselId}", engineId, vesselId);
            await _unitOfWork.RollbackTransactionAsync();
            return false;
        }
    }

    public async Task<bool> StopEngineAsync(string vesselId, string engineId)
    {
        _logger.LogInformation("Stopping engine {EngineId} for vessel {VesselId}", engineId, vesselId);
        
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var engine = await GetEngineByIdAsync(vesselId, engineId);
            if (engine == null)
            {
                _logger.LogWarning("Engine {EngineId} not found for vessel {VesselId}", engineId, vesselId);
                return false;
            }

            if (engine.Status == EngineStatus.Stopped)
            {
                _logger.LogInformation("Engine {EngineId} is already stopped", engineId);
                return true;
            }

            // Update engine status
            engine.Status = EngineStatus.Stopped;
            engine.IsRunning = false;
            engine.RPM = 0;
            engine.LastUpdated = DateTime.UtcNow;
            
            _unitOfWork.Engines.Update(engine);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation("Engine {EngineId} stopped successfully", engineId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop engine {EngineId} for vessel {VesselId}", engineId, vesselId);
            await _unitOfWork.RollbackTransactionAsync();
            return false;
        }
    }

    public async Task<bool> SetEngineRPMAsync(string vesselId, string engineId, int rpm)
    {
        _logger.LogInformation("Setting RPM to {RPM} for engine {EngineId} on vessel {VesselId}", rpm, engineId, vesselId);
        
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var engine = await GetEngineByIdAsync(vesselId, engineId);
            if (engine == null)
            {
                _logger.LogWarning("Engine {EngineId} not found for vessel {VesselId}", engineId, vesselId);
                return false;
            }

            if (engine.Status != EngineStatus.Running)
            {
                _logger.LogWarning("Cannot set RPM for engine {EngineId} - engine is not running", engineId);
                return false;
            }

            if (rpm < 0 || rpm > engine.MaxRPM)
            {
                _logger.LogWarning("Invalid RPM {RPM} for engine {EngineId} - must be between 0 and {MaxRPM}", rpm, engineId, engine.MaxRPM);
                return false;
            }

            // Update engine RPM
            engine.RPM = rpm;
            engine.LastUpdated = DateTime.UtcNow;
            
            _unitOfWork.Engines.Update(engine);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation("RPM set to {RPM} for engine {EngineId} successfully", rpm, engineId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set RPM for engine {EngineId} on vessel {VesselId}", engineId, vesselId);
            await _unitOfWork.RollbackTransactionAsync();
            return false;
        }
    }

    public async Task<IEnumerable<Sensor>> GetVesselSensorsAsync(string vesselId)
    {
        _logger.LogInformation("Retrieving sensors for vessel {VesselId} from database", vesselId);
        
        // For this example, we'll return all active sensors
        // In a real implementation, you might have a relationship between vessels and sensors
        return await _unitOfWork.Sensors.GetActiveSensorsAsync();
    }

    public async Task<Sensor?> GetSensorByIdAsync(string vesselId, string sensorId)
    {
        _logger.LogInformation("Retrieving sensor {SensorId} for vessel {VesselId} from database", sensorId, vesselId);
        return await _unitOfWork.Sensors.GetByIdAsync(sensorId);
    }
}
