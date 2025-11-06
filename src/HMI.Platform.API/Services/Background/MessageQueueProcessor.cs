using System.Collections.Concurrent;
using HMI.Platform.Core.Services;
using Microsoft.Extensions.Options;

namespace HMI.Platform.API.Services.Background;

/// <summary>
/// Background service for processing messages from a queue.
/// </summary>
public class MessageQueueProcessor : BackgroundServiceBase
{
    private readonly ConcurrentQueue<QueueMessage> _messageQueue = new();
    private readonly MessageQueueOptions _options;
    private readonly SemaphoreSlim _processingSemaphore;

    public MessageQueueProcessor(
        ILogger<MessageQueueProcessor> logger,
        IServiceProvider serviceProvider,
        IOptions<MessageQueueOptions> options)
        : base(logger, serviceProvider)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _processingSemaphore = new SemaphoreSlim(_options.MaxConcurrentMessages, _options.MaxConcurrentMessages);
    }

    /// <summary>
    /// Enqueues a message for processing.
    /// </summary>
    public void Enqueue(QueueMessage message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        _messageQueue.Enqueue(message);
        Logger.LogDebug("Message enqueued: {MessageId}", message.Id);
    }

    protected override async Task ExecuteWorkAsync(CancellationToken cancellationToken)
    {
        if (_messageQueue.IsEmpty)
        {
            // No messages, wait a bit before checking again
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            return;
        }

        if (!await _processingSemaphore.WaitAsync(0, cancellationToken))
        {
            // Max concurrent messages reached, wait
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            return;
        }

        try
        {
            if (_messageQueue.TryDequeue(out var message))
            {
                await ProcessMessageAsync(message, cancellationToken);
            }
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }

    private async Task ProcessMessageAsync(QueueMessage message, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Processing message: {MessageId}, Type: {MessageType}", message.Id, message.Type);

        try
        {
            using var scope = ServiceProvider.CreateScope();
            
            switch (message.Type)
            {
                case MessageType.VesselUpdate:
                    await ProcessVesselUpdateAsync(message, scope, cancellationToken);
                    break;

                case MessageType.EngineControl:
                    await ProcessEngineControlAsync(message, scope, cancellationToken);
                    break;

                case MessageType.AlarmNotification:
                    await ProcessAlarmNotificationAsync(message, scope, cancellationToken);
                    break;

                default:
                    Logger.LogWarning("Unknown message type: {MessageType}", message.Type);
                    break;
            }

            Logger.LogDebug("Message processed successfully: {MessageId}", message.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing message: {MessageId}", message.Id);
            
            // Retry logic
            if (message.RetryCount < _options.MaxRetries)
            {
                message.RetryCount++;
                _messageQueue.Enqueue(message);
                Logger.LogInformation("Message requeued for retry: {MessageId}, Retry: {RetryCount}", message.Id, message.RetryCount);
            }
            else
            {
                Logger.LogError("Message failed after {MaxRetries} retries: {MessageId}", _options.MaxRetries, message.Id);
                await OnMessageFailedAsync(message, ex);
            }
        }
    }

    private async Task ProcessVesselUpdateAsync(QueueMessage message, IServiceScope scope, CancellationToken cancellationToken)
    {
        // Process vessel update message
        Logger.LogDebug("Processing vessel update: {MessageId}", message.Id);
        await Task.CompletedTask;
    }

    private async Task ProcessEngineControlAsync(QueueMessage message, IServiceScope scope, CancellationToken cancellationToken)
    {
        // Process engine control message
        Logger.LogDebug("Processing engine control: {MessageId}", message.Id);
        await Task.CompletedTask;
    }

    private async Task ProcessAlarmNotificationAsync(QueueMessage message, IServiceScope scope, CancellationToken cancellationToken)
    {
        // Process alarm notification message
        Logger.LogDebug("Processing alarm notification: {MessageId}", message.Id);
        await Task.CompletedTask;
    }

    private async Task OnMessageFailedAsync(QueueMessage message, Exception exception)
    {
        // Handle failed message (e.g., send to dead letter queue)
        Logger.LogError("Message failed permanently: {MessageId}", message.Id);
        await Task.CompletedTask;
    }

    protected override TimeSpan GetDelayInterval()
    {
        return TimeSpan.Zero; // Process continuously
    }

    protected override Task OnStartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Message queue processor started");
        return Task.CompletedTask;
    }

    protected override Task OnStopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Message queue processor stopped. Queue size: {QueueSize}", _messageQueue.Count);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Represents a message in the queue.
/// </summary>
public class QueueMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public MessageType Type { get; set; }
    public string Payload { get; set; } = string.Empty;
    public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// Message types.
/// </summary>
public enum MessageType
{
    VesselUpdate,
    EngineControl,
    AlarmNotification,
    DataSync,
    HealthCheck
}

/// <summary>
/// Options for message queue processor.
/// </summary>
public class MessageQueueOptions
{
    public int MaxConcurrentMessages { get; set; } = 5;
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
}

