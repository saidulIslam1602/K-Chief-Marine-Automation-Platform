using HMI.Platform.Core.Interfaces;
using HMI.Platform.Core.Models;
using Microsoft.Extensions.Logging;

namespace HMI.VesselControl.Services;

/// <summary>
/// MODERNIZED CODE - This demonstrates how the legacy code was modernized.
/// 
/// Improvements:
/// 1. Dependency injection instead of static methods
/// 2. Async/await for non-blocking operations
/// 3. Proper error handling with exceptions
/// 4. Strongly typed return values
/// 5. Logging for observability
/// 6. Configuration instead of magic numbers
/// 7. Unit testable design
/// 8. Separation of concerns
/// </summary>
public class ModernizedVesselController
{
    private readonly IVesselControlService _vesselControlService;
    private readonly ILogger<ModernizedVesselController> _logger;
    private const int MaxRpm = 1000; // Configuration constant instead of magic number

    // Dependency injection - testable and maintainable
    public ModernizedVesselController(
        IVesselControlService vesselControlService,
        ILogger<ModernizedVesselController> logger)
    {
        _vesselControlService = vesselControlService ?? throw new ArgumentNullException(nameof(vesselControlService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a vessel asynchronously with proper error handling.
    /// </summary>
    public async Task<Vessel?> GetVesselAsync(string vesselId)
    {
        if (string.IsNullOrWhiteSpace(vesselId))
        {
            _logger.LogWarning("Attempted to get vessel with invalid ID");
            throw new ArgumentException("Vessel ID cannot be null or empty", nameof(vesselId));
        }

        try
        {
            _logger.LogInformation("Retrieving vessel with ID: {VesselId}", vesselId);
            var vessel = await _vesselControlService.GetVesselByIdAsync(vesselId);
            
            if (vessel == null)
            {
                _logger.LogWarning("Vessel not found: {VesselId}", vesselId);
            }

            return vessel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vessel: {VesselId}", vesselId);
            throw; // Re-throw with context
        }
    }

    /// <summary>
    /// Sets engine RPM with validation and proper error handling.
    /// </summary>
    public async Task<bool> SetEngineRPMAsync(string vesselId, string engineId, int rpm)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(vesselId))
        {
            throw new ArgumentException("Vessel ID cannot be null or empty", nameof(vesselId));
        }

        if (string.IsNullOrWhiteSpace(engineId))
        {
            throw new ArgumentException("Engine ID cannot be null or empty", nameof(engineId));
        }

        if (rpm < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rpm), "RPM cannot be negative");
        }

        if (rpm > MaxRpm)
        {
            _logger.LogWarning("Attempted to set RPM {Rpm} exceeding maximum {MaxRpm} for engine {EngineId}", 
                rpm, MaxRpm, engineId);
            throw new ArgumentOutOfRangeException(nameof(rpm), $"RPM cannot exceed {MaxRpm}");
        }

        try
        {
            _logger.LogInformation("Setting RPM to {Rpm} for engine {EngineId} on vessel {VesselId}", 
                rpm, engineId, vesselId);

            var result = await _vesselControlService.SetEngineRPMAsync(vesselId, engineId, rpm);

            if (result)
            {
                _logger.LogInformation("Successfully set RPM to {Rpm} for engine {EngineId}", rpm, engineId);
            }
            else
            {
                _logger.LogWarning("Failed to set RPM for engine {EngineId}. Engine may not be running.", engineId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting RPM for engine {EngineId} on vessel {VesselId}", engineId, vesselId);
            throw;
        }
    }

    /// <summary>
    /// Processes vessel data with proper parsing and validation.
    /// </summary>
    public async Task<VesselData?> ProcessVesselDataAsync(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            throw new ArgumentException("Data cannot be null or empty", nameof(data));
        }

        try
        {
            var parts = data.Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length < 2)
            {
                _logger.LogWarning("Invalid vessel data format. Expected at least 2 parts, got {Count}", parts.Length);
                return null;
            }

            // Proper parsing with validation
            if (!int.TryParse(parts[1], out var rpm))
            {
                _logger.LogWarning("Invalid RPM value in vessel data: {Value}", parts[1]);
                return null;
            }

            var vesselData = new VesselData
            {
                VesselId = parts[0],
                Rpm = rpm
            };

            _logger.LogInformation("Processed vessel data for vessel {VesselId}", vesselData.VesselId);
            return await Task.FromResult(vesselData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing vessel data");
            throw;
        }
    }
}

/// <summary>
/// Strongly typed data model instead of anonymous objects.
/// </summary>
public class VesselData
{
    public string VesselId { get; set; } = string.Empty;
    public int Rpm { get; set; }
}

