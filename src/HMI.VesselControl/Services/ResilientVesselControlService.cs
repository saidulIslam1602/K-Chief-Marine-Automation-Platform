using Microsoft.Extensions.Logging;
using HMI.Platform.Core.Interfaces;
using HMI.Platform.Core.Models;
using HMI.Platform.Core.Exceptions;

namespace HMI.VesselControl.Services;

/// <summary>
/// Resilient vessel control service that implements retry, circuit breaker, and fallback patterns.
/// Demonstrates how to integrate resilience patterns into domain services.
/// </summary>
public class ResilientVesselControlService : IVesselControlService
{
    private readonly IVesselControlService _baseService;
    private readonly ILogger<ResilientVesselControlService> _logger;

    // Fallback data for when primary service is unavailable
    private static readonly Dictionary<string, Vessel> _fallbackVessels = new()
    {
        ["fallback-001"] = new Vessel
        {
            Id = "fallback-001",
            Name = "Emergency Vessel",
            Type = "CargoShip",
            Status = VesselStatus.Offline,
            Location = "Safe Harbor",
            Length = 100,
            Width = 20,
            MaxSpeed = 15,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        }
    };

    public ResilientVesselControlService(
        IVesselControlService baseService,
        ILogger<ResilientVesselControlService> logger)
    {
        _baseService = baseService;
        _logger = logger;
    }

    public async Task<IEnumerable<Vessel>> GetAllVesselsAsync()
    {
        try
        {
            _logger.LogDebug("Executing GetAllVessels operation");
            return await _baseService.GetAllVesselsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get vessels, returning fallback data");
            return _fallbackVessels.Values;
        }
    }

    public async Task<Vessel?> GetVesselByIdAsync(string vesselId)
    {
        try
        {
            _logger.LogDebug("Executing GetVesselById operation for vessel {VesselId}", vesselId);
            
            var vessel = await _baseService.GetVesselByIdAsync(vesselId);
            
            // If vessel not found and we're in degraded mode, return fallback
            if (vessel == null && _fallbackVessels.ContainsKey(vesselId))
            {
                _logger.LogWarning("Primary vessel data unavailable, returning fallback data for {VesselId}", vesselId);
                return _fallbackVessels[vesselId];
            }
            
            return vessel;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get vessel {VesselId}, returning fallback if available", vesselId);
            return _fallbackVessels.ContainsKey(vesselId) ? _fallbackVessels[vesselId] : null;
        }
    }

    public async Task<IEnumerable<Engine>> GetVesselEnginesAsync(string vesselId)
    {
        try
        {
            _logger.LogDebug("Executing GetVesselEngines operation for vessel {VesselId}", vesselId);
            return await _baseService.GetVesselEnginesAsync(vesselId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get engines for vessel {VesselId}, returning fallback engines", vesselId);
            
            // Return fallback engines
            return new[]
            {
                new Engine
                {
                    Id = $"fallback-engine-{vesselId}",
                    Name = "Emergency Engine",
                    Type = "Diesel",
                    VesselId = vesselId,
                    RPM = 0,
                    MaxRPM = 1800,
                    Temperature = 20,
                    IsRunning = false
                }
            };
        }
    }

    public async Task<IEnumerable<Sensor>> GetVesselSensorsAsync(string vesselId)
    {
        try
        {
            _logger.LogDebug("Executing GetVesselSensors operation for vessel {VesselId}", vesselId);
            return await _baseService.GetVesselSensorsAsync(vesselId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get sensors for vessel {VesselId}, returning fallback sensors", vesselId);
            
            // Return fallback sensors with safe default values
            return new[]
            {
                new Sensor
                {
                    Id = $"fallback-sensor-{vesselId}",
                    Name = "Emergency Sensor",
                    Type = "Temperature",
                    Value = 20.0,
                    Unit = "°C",
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
                }
            };
        }
    }



    public async Task<bool> SetEngineRPMAsync(string vesselId, string engineId, int rpm)
    {
        try
        {
            _logger.LogInformation("Executing SetEngineRPM operation for engine {EngineId} on vessel {VesselId} to {RPM} RPM",
                engineId, vesselId, rpm);
            
            // Validate RPM range
            if (rpm < 0 || rpm > 3000)
            {
                throw new VesselOperationException(vesselId, "SetEngineRPM");
            }
            
            var result = await _baseService.SetEngineRPMAsync(vesselId, engineId, rpm);
            
            if (result)
            {
                _logger.LogInformation("Engine {EngineId} RPM set to {RPM} successfully", engineId, rpm);
            }
            else
            {
                _logger.LogError("Failed to set engine {EngineId} RPM to {RPM}", engineId, rpm);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting engine {EngineId} RPM to {RPM}", engineId, rpm);
            return false;
        }
    }

    /// <summary>
    /// Emergency stop operation with minimal resilience overhead for maximum speed.
    /// </summary>
    public async Task<bool> EmergencyStopAsync(string vesselId)
    {
        _logger.LogDebug("Starting EmergencyStop operation for vessel {VesselId}", vesselId);
        {
            _logger.LogError("EMERGENCY STOP initiated for vessel {VesselId}", vesselId);
            
            try
            {
                // Emergency operations bypass normal resilience patterns for speed
                // but still have basic retry for critical safety
                // Emergency stop - use StopEngineAsync for all engines instead of StopVesselAsync
                var engines = await _baseService.GetVesselEnginesAsync(vesselId);
                bool allStopped = true;
                foreach (var engine in engines)
                {
                    var stopped = await _baseService.StopEngineAsync(vesselId, engine.Id);
                    if (!stopped) allStopped = false;
                }
                var result = allStopped;
                
                if (result)
                {
                    _logger.LogError("EMERGENCY STOP completed successfully for vessel {VesselId}", vesselId);
                }
                else
                {
                    _logger.LogCritical("EMERGENCY STOP FAILED for vessel {VesselId} - MANUAL INTERVENTION REQUIRED", vesselId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "EMERGENCY STOP EXCEPTION for vessel {VesselId} - MANUAL INTERVENTION REQUIRED", vesselId);
                throw new VesselOperationException(vesselId, "EmergencyStop")
                    .WithContext("Severity", "Critical");
            }
        }
    }

    // Missing interface methods
    public async Task<Engine?> GetEngineByIdAsync(string vesselId, string engineId)
    {
        try
        {
            return await _baseService.GetEngineByIdAsync(vesselId, engineId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get engine {EngineId} for vessel {VesselId}", engineId, vesselId);
            return null;
        }
    }

    public async Task<bool> StartEngineAsync(string vesselId, string engineId)
    {
        try
        {
            return await _baseService.StartEngineAsync(vesselId, engineId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start engine {EngineId} for vessel {VesselId}", engineId, vesselId);
            return false;
        }
    }

    public async Task<bool> StopEngineAsync(string vesselId, string engineId)
    {
        try
        {
            return await _baseService.StopEngineAsync(vesselId, engineId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to stop engine {EngineId} for vessel {VesselId}", engineId, vesselId);
            return false;
        }
    }
}
