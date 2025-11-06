using KChief.Platform.Core.Interfaces;
using KChief.Platform.Core.Models;
using KChief.Platform.Core.Services;
using Microsoft.Extensions.Options;

namespace KChief.Platform.API.Services.Background;

/// <summary>
/// Background service for synchronizing data between systems.
/// </summary>
public class DataSynchronizationService : BackgroundServiceBase
{
    private readonly IVesselControlService _vesselControlService;
    private readonly IAlarmService _alarmService;
    private readonly DataSynchronizationOptions _options;

    public DataSynchronizationService(
        ILogger<DataSynchronizationService> logger,
        IServiceProvider serviceProvider,
        IVesselControlService vesselControlService,
        IAlarmService alarmService,
        IOptions<DataSynchronizationOptions> options)
        : base(logger, serviceProvider)
    {
        _vesselControlService = vesselControlService ?? throw new ArgumentNullException(nameof(vesselControlService));
        _alarmService = alarmService ?? throw new ArgumentNullException(nameof(alarmService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteWorkAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("Starting data synchronization cycle");

        try
        {
            if (_options.SyncVessels)
            {
                await SynchronizeVesselsAsync(cancellationToken);
            }

            if (_options.SyncAlarms)
            {
                await SynchronizeAlarmsAsync(cancellationToken);
            }

            if (_options.SyncEngineStatus)
            {
                await SynchronizeEngineStatusAsync(cancellationToken);
            }

            Logger.LogDebug("Data synchronization cycle completed");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during data synchronization cycle");
            throw;
        }
    }

    private async Task SynchronizeVesselsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var vessels = await _vesselControlService.GetAllVesselsAsync();
            Logger.LogDebug("Synchronized {Count} vessels", vessels.Count());
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error synchronizing vessels");
        }
    }

    private async Task SynchronizeAlarmsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var alarms = await _alarmService.GetAllAlarmsAsync();
            var activeAlarms = alarms.Where(a => a.Status == AlarmStatus.Active);
            Logger.LogDebug("Synchronized {Count} active alarms", activeAlarms.Count());
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error synchronizing alarms");
        }
    }

    private async Task SynchronizeEngineStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            var vessels = await _vesselControlService.GetAllVesselsAsync();
            int engineCount = 0;

            foreach (var vessel in vessels)
            {
                var engines = await _vesselControlService.GetVesselEnginesAsync(vessel.Id);
                engineCount += engines.Count();
            }

            Logger.LogDebug("Synchronized status for {Count} engines", engineCount);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error synchronizing engine status");
        }
    }

    protected override TimeSpan GetDelayInterval()
    {
        return _options.SyncInterval;
    }

    protected override Task OnStartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Data synchronization service started with interval {Interval}", _options.SyncInterval);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Options for data synchronization service.
/// </summary>
public class DataSynchronizationOptions
{
    public TimeSpan SyncInterval { get; set; } = TimeSpan.FromMinutes(10);
    public bool SyncVessels { get; set; } = true;
    public bool SyncAlarms { get; set; } = true;
    public bool SyncEngineStatus { get; set; } = true;
}

