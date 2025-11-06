using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HMI.Platform.Core.Services;

/// <summary>
/// Base class for background services with common functionality.
/// </summary>
public abstract class BackgroundServiceBase : BackgroundService
{
    protected readonly ILogger Logger;
    protected readonly IServiceProvider ServiceProvider;

    protected BackgroundServiceBase(
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("{ServiceName} is starting", GetType().Name);

        try
        {
            await OnStartAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteWorkAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error in {ServiceName} execution", GetType().Name);
                    await OnErrorAsync(ex, stoppingToken);
                }

                var delay = GetDelayInterval();
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("{ServiceName} is stopping", GetType().Name);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "{ServiceName} crashed", GetType().Name);
            throw;
        }
        finally
        {
            await OnStopAsync(stoppingToken);
        }
    }

    /// <summary>
    /// Called when the service starts.
    /// </summary>
    protected virtual Task OnStartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the service stops.
    /// </summary>
    protected virtual Task OnStopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when an error occurs during execution.
    /// </summary>
    protected virtual Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the main work of the service.
    /// </summary>
    protected abstract Task ExecuteWorkAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the delay interval between executions.
    /// </summary>
    protected abstract TimeSpan GetDelayInterval();
}

