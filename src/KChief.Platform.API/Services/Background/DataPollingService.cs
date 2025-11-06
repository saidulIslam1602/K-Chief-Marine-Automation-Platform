using KChief.Platform.Core.Interfaces;
using KChief.Platform.Core.Models;
using KChief.Platform.Core.Services;
using Microsoft.Extensions.Options;

namespace KChief.Platform.API.Services.Background;

/// <summary>
/// Background service for polling sensor data from vessels.
/// </summary>
public class DataPollingService : BackgroundServiceBase
{
    private readonly IVesselControlService _vesselControlService;
    private readonly IAlarmService _alarmService;
    private readonly DataPollingOptions _options;

    public DataPollingService(
        ILogger<DataPollingService> logger,
        IServiceProvider serviceProvider,
        IVesselControlService vesselControlService,
        IAlarmService alarmService,
        IOptions<DataPollingOptions> options)
        : base(logger, serviceProvider)
    {
        _vesselControlService = vesselControlService ?? throw new ArgumentNullException(nameof(vesselControlService));
        _alarmService = alarmService ?? throw new ArgumentNullException(nameof(alarmService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteWorkAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("Starting data polling cycle");

        try
        {
            var vessels = await _vesselControlService.GetAllVesselsAsync();
            
            foreach (var vessel in vessels)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await PollVesselDataAsync(vessel, cancellationToken);
            }

            Logger.LogDebug("Data polling cycle completed");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during data polling cycle");
            throw;
        }
    }

    private async Task PollVesselDataAsync(Vessel vessel, CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogDebug("Polling data for vessel {VesselId}", vessel.Id);

            // Poll sensors
            var sensors = await _vesselControlService.GetVesselSensorsAsync(vessel.Id);
            foreach (var sensor in sensors)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // Evaluate sensor value against alarm rules
                if (sensor.Value.HasValue)
                {
                    await _alarmService.EvaluateSensorValueAsync(
                        sensor.Id,
                        sensor.Value.Value,
                        vessel.Id);
                }
            }

            // Poll engines
            var engines = await _vesselControlService.GetVesselEnginesAsync(vessel.Id);
            foreach (var engine in engines)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // Evaluate engine status against alarm rules
                await _alarmService.EvaluateEngineStatusAsync(
                    engine.Id,
                    engine.Status,
                    new Dictionary<string, double>
                    {
                        ["Temperature"] = engine.Temperature,
                        ["Pressure"] = engine.Pressure,
                        ["RPM"] = engine.CurrentRpm
                    },
                    vessel.Id);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error polling data for vessel {VesselId}", vessel.Id);
        }
    }

    protected override TimeSpan GetDelayInterval()
    {
        return _options.PollingInterval;
    }

    protected override Task OnStartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Data polling service started with interval {Interval}", _options.PollingInterval);
        return Task.CompletedTask;
    }

    protected override Task OnStopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Data polling service stopped");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Options for data polling service.
/// </summary>
public class DataPollingOptions
{
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxConcurrentVessels { get; set; } = 10;
    public bool EnableSensorPolling { get; set; } = true;
    public bool EnableEnginePolling { get; set; } = true;
}

