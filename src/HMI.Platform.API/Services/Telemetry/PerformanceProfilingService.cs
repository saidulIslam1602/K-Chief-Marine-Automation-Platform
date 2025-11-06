using System.Collections.Concurrent;
using System.Diagnostics;
using HMI.Platform.Core.Telemetry;

namespace HMI.Platform.API.Services.Telemetry;

/// <summary>
/// Service for performance profiling and analysis.
/// </summary>
public class PerformanceProfilingService
{
    private readonly ITelemetryService _telemetryService;
    private readonly ILogger<PerformanceProfilingService> _logger;
    private readonly ConcurrentDictionary<string, ProfilingSession> _sessions = new();

    public PerformanceProfilingService(
        ITelemetryService telemetryService,
        ILogger<PerformanceProfilingService> logger)
    {
        _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Starts a profiling session.
    /// </summary>
    public string StartProfiling(string operationName, IDictionary<string, string>? metadata = null)
    {
        var sessionId = Guid.NewGuid().ToString();
        var session = new ProfilingSession
        {
            SessionId = sessionId,
            OperationName = operationName,
            StartTime = DateTime.UtcNow,
            Stopwatch = Stopwatch.StartNew(),
            Metadata = metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>()
        };

        _sessions[sessionId] = session;

        _logger.LogDebug("Started profiling session: {SessionId} - {OperationName}", sessionId, operationName);

        return sessionId;
    }

    /// <summary>
    /// Records a checkpoint in the profiling session.
    /// </summary>
    public void RecordCheckpoint(string sessionId, string checkpointName, IDictionary<string, string>? data = null)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            _logger.LogWarning("Profiling session not found: {SessionId}", sessionId);
            return;
        }

        var checkpoint = new ProfilingCheckpoint
        {
            Name = checkpointName,
            Timestamp = DateTime.UtcNow,
            ElapsedMilliseconds = session.Stopwatch.ElapsedMilliseconds,
            Data = data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>()
        };

        session.Checkpoints.Add(checkpoint);

        _logger.LogDebug("Recorded checkpoint: {SessionId} - {CheckpointName} ({ElapsedMs}ms)",
            sessionId, checkpointName, checkpoint.ElapsedMilliseconds);
    }

    /// <summary>
    /// Stops a profiling session and returns the results.
    /// </summary>
    public ProfilingResult StopProfiling(string sessionId)
    {
        if (!_sessions.TryRemove(sessionId, out var session))
        {
            _logger.LogWarning("Profiling session not found: {SessionId}", sessionId);
            return new ProfilingResult
            {
                SessionId = sessionId,
                Success = false,
                ErrorMessage = "Session not found"
            };
        }

        session.Stopwatch.Stop();
        var duration = session.Stopwatch.Elapsed;

        var result = new ProfilingResult
        {
            SessionId = sessionId,
            OperationName = session.OperationName,
            StartTime = session.StartTime,
            EndTime = DateTime.UtcNow,
            Duration = duration,
            Checkpoints = session.Checkpoints,
            Metadata = session.Metadata,
            Success = true
        };

        // Track in Application Insights
        _telemetryService.TrackMetric($"Profiling.{session.OperationName}.Duration", duration.TotalMilliseconds, session.Metadata);

        // Log slow operations
        if (duration.TotalSeconds > 1.0)
        {
            _logger.LogWarning(
                "Slow operation detected: {OperationName} took {DurationMs}ms - Session: {SessionId}",
                session.OperationName, duration.TotalMilliseconds, sessionId);

            _telemetryService.TrackEvent("SlowOperation", new Dictionary<string, string>
            {
                ["operation"] = session.OperationName,
                ["duration_ms"] = duration.TotalMilliseconds.ToString("F2"),
                ["session_id"] = sessionId
            });
        }

        _logger.LogInformation(
            "Completed profiling session: {SessionId} - {OperationName} ({DurationMs}ms, {CheckpointCount} checkpoints)",
            sessionId, session.OperationName, duration.TotalMilliseconds, session.Checkpoints.Count);

        return result;
    }

    /// <summary>
    /// Profiles an async operation.
    /// </summary>
    public async Task<T> ProfileAsync<T>(string operationName, Func<Task<T>> operation, IDictionary<string, string>? metadata = null)
    {
        var sessionId = StartProfiling(operationName, metadata);
        try
        {
            RecordCheckpoint(sessionId, "Started");
            var result = await operation();
            RecordCheckpoint(sessionId, "Completed");
            return result;
        }
        catch (Exception ex)
        {
            RecordCheckpoint(sessionId, "Error", new Dictionary<string, string>
            {
                ["error"] = ex.Message,
                ["error_type"] = ex.GetType().Name
            });
            throw;
        }
        finally
        {
            StopProfiling(sessionId);
        }
    }

    /// <summary>
    /// Profiles a sync operation.
    /// </summary>
    public T Profile<T>(string operationName, Func<T> operation, IDictionary<string, string>? metadata = null)
    {
        var sessionId = StartProfiling(operationName, metadata);
        try
        {
            RecordCheckpoint(sessionId, "Started");
            var result = operation();
            RecordCheckpoint(sessionId, "Completed");
            return result;
        }
        catch (Exception ex)
        {
            RecordCheckpoint(sessionId, "Error", new Dictionary<string, string>
            {
                ["error"] = ex.Message,
                ["error_type"] = ex.GetType().Name
            });
            throw;
        }
        finally
        {
            StopProfiling(sessionId);
        }
    }
}

/// <summary>
/// Profiling session.
/// </summary>
internal class ProfilingSession
{
    public string SessionId { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public Stopwatch Stopwatch { get; set; } = null!;
    public List<ProfilingCheckpoint> Checkpoints { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Profiling checkpoint.
/// </summary>
public class ProfilingCheckpoint
{
    public string Name { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
}

/// <summary>
/// Profiling result.
/// </summary>
public class ProfilingResult
{
    public string SessionId { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public List<ProfilingCheckpoint> Checkpoints { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

