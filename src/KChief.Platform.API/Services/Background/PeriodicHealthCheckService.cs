using KChief.Platform.Core.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace KChief.Platform.API.Services.Background;

/// <summary>
/// Background service for periodic health checks.
/// </summary>
public class PeriodicHealthCheckService : BackgroundServiceBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly PeriodicHealthCheckOptions _options;

    public PeriodicHealthCheckService(
        ILogger<PeriodicHealthCheckService> logger,
        IServiceProvider serviceProvider,
        HealthCheckService healthCheckService,
        IOptions<PeriodicHealthCheckOptions> options)
        : base(logger, serviceProvider)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteWorkAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("Starting periodic health check");

        try
        {
            var result = await _healthCheckService.CheckHealthAsync(
                predicate: check => check.Tags.Contains("periodic"),
                cancellationToken: cancellationToken);

            if (result.Status == HealthStatus.Healthy)
            {
                Logger.LogDebug("Periodic health check passed");
            }
            else if (result.Status == HealthStatus.Degraded)
            {
                Logger.LogWarning("Periodic health check degraded: {Status}", result.Status);
            }
            else
            {
                Logger.LogError("Periodic health check failed: {Status}", result.Status);
                
                foreach (var entry in result.Entries)
                {
                    if (entry.Value.Status != HealthStatus.Healthy)
                    {
                        Logger.LogError(
                            "Health check '{Name}' failed: {Description}",
                            entry.Key, entry.Value.Description);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during periodic health check");
        }
    }

    protected override TimeSpan GetDelayInterval()
    {
        return _options.CheckInterval;
    }

    protected override Task OnStartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Periodic health check service started with interval {Interval}", _options.CheckInterval);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Options for periodic health check service.
/// </summary>
public class PeriodicHealthCheckOptions
{
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableDetailedLogging { get; set; } = true;
}

