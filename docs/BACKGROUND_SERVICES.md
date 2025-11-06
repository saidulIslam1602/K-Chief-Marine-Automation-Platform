# Background Services and Hosted Services

## Overview

The HMI Marine Automation Platform includes comprehensive background processing capabilities using ASP.NET Core hosted services and Quartz.NET for scheduled tasks.

## Architecture

### Background Service Components

```
Background Services
├── BackgroundServiceBase          # Base class for background services
├── DataPollingService             # Polls sensor and engine data
├── PeriodicHealthCheckService     # Periodic health monitoring
├── DataSynchronizationService     # Data synchronization
├── MessageQueueProcessor          # Message queue processing
└── ScheduledTaskService           # Quartz.NET scheduler
```

## Background Services

### Base Background Service

All background services inherit from `BackgroundServiceBase`:

```csharp
public abstract class BackgroundServiceBase : BackgroundService
{
    protected abstract Task ExecuteWorkAsync(CancellationToken cancellationToken);
    protected abstract TimeSpan GetDelayInterval();
    
    protected virtual Task OnStartAsync(CancellationToken cancellationToken);
    protected virtual Task OnStopAsync(CancellationToken cancellationToken);
    protected virtual Task OnErrorAsync(Exception exception, CancellationToken cancellationToken);
}
```

### Data Polling Service

Polls sensor and engine data from vessels at regular intervals:

**Configuration:**
```json
{
  "BackgroundServices": {
    "DataPolling": {
      "PollingInterval": "00:00:30",
      "MaxConcurrentVessels": 10,
      "EnableSensorPolling": true,
      "EnableEnginePolling": true
    }
  }
}
```

**Features:**
- Polls sensor values from all vessels
- Evaluates sensor values against alarm rules
- Polls engine status and metrics
- Evaluates engine status against alarm rules
- Configurable polling interval
- Concurrent vessel processing

**Usage:**
The service automatically starts when the application starts and runs continuously.

### Periodic Health Check Service

Performs periodic health checks on system components:

**Configuration:**
```json
{
  "BackgroundServices": {
    "PeriodicHealthCheck": {
      "CheckInterval": "00:05:00",
      "EnableDetailedLogging": true
    }
  }
}
```

**Features:**
- Periodic health check execution
- Detailed logging of health status
- Degraded and unhealthy state detection
- Component-level health reporting

### Data Synchronization Service

Synchronizes data between systems and components:

**Configuration:**
```json
{
  "BackgroundServices": {
    "DataSynchronization": {
      "SyncInterval": "00:10:00",
      "SyncVessels": true,
      "SyncAlarms": true,
      "SyncEngineStatus": true
    }
  }
}
```

**Features:**
- Vessel data synchronization
- Alarm synchronization
- Engine status synchronization
- Configurable sync intervals
- Selective synchronization

### Message Queue Processor

Processes messages from an internal queue:

**Configuration:**
```json
{
  "BackgroundServices": {
    "MessageQueue": {
      "MaxConcurrentMessages": 5,
      "MaxRetries": 3,
      "RetryDelay": "00:00:05"
    }
  }
}
```

**Features:**
- In-memory message queue
- Concurrent message processing
- Automatic retry on failure
- Dead letter queue support
- Message type routing

**Usage:**
```csharp
// Enqueue a message
var message = new QueueMessage
{
    Type = MessageType.VesselUpdate,
    Payload = JsonSerializer.Serialize(vesselData)
};

messageQueueProcessor.Enqueue(message);
```

**Message Types:**
- `VesselUpdate`: Vessel data updates
- `EngineControl`: Engine control commands
- `AlarmNotification`: Alarm notifications
- `DataSync`: Data synchronization requests
- `HealthCheck`: Health check requests

## Scheduled Tasks (Quartz.NET)

### Configuration

Quartz.NET is configured in `Program.cs`:

```csharp
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore();
    q.UseDefaultThreadPool(tp =>
    {
        tp.MaxConcurrency = 10;
    });
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
```

### Scheduled Jobs

#### Data Cleanup Job

Runs daily at 2 AM to clean up old data:

```csharp
[DisallowConcurrentExecution]
public class DataCleanupJob : ScheduledJobBase
{
    protected override async Task ExecuteJobAsync(IJobExecutionContext context)
    {
        // Cleanup old alarm history
        // Cleanup old sensor readings
        // Cleanup old logs
    }
}
```

**Schedule:** Daily at 2:00 AM (Cron: `0 0 2 * * ?`)

#### Report Generation Job

Generates daily reports:

```csharp
[DisallowConcurrentExecution]
public class ReportGenerationJob : ScheduledJobBase
{
    protected override async Task ExecuteJobAsync(IJobExecutionContext context)
    {
        // Generate daily reports
        // Generate weekly summaries
        // Generate monthly analytics
    }
}
```

**Schedule:** Daily at midnight (Cron: `0 0 0 * * ?`)

#### Health Check Job

Runs periodic health checks:

```csharp
public class HealthCheckJob : ScheduledJobBase
{
    protected override async Task ExecuteJobAsync(IJobExecutionContext context)
    {
        var result = await _healthCheckService.CheckHealthAsync();
        // Log results
    }
}
```

**Schedule:** Every 5 minutes

### Creating Custom Scheduled Jobs

```csharp
[DisallowConcurrentExecution]
public class MyCustomJob : ScheduledJobBase
{
    public MyCustomJob(ILogger<MyCustomJob> logger) : base(logger) { }

    protected override async Task ExecuteJobAsync(IJobExecutionContext context)
    {
        Logger.LogInformation("Executing custom job");
        // Your job logic here
        await Task.CompletedTask;
    }
}

// Register in Program.cs
builder.Services.AddScoped<MyCustomJob>();

// Schedule in ScheduledTaskService
var job = JobBuilder.Create<MyCustomJob>()
    .WithIdentity("my-custom-job", "custom")
    .Build();

var trigger = TriggerBuilder.Create()
    .WithIdentity("my-custom-trigger", "custom")
    .StartNow()
    .WithCronSchedule("0 */15 * * * ?") // Every 15 minutes
    .Build();

await _scheduler.ScheduleJob(job, trigger, cancellationToken);
```

### Cron Expression Examples

| Expression | Description |
|------------|-------------|
| `0 0 0 * * ?` | Daily at midnight |
| `0 0 2 * * ?` | Daily at 2 AM |
| `0 */15 * * * ?` | Every 15 minutes |
| `0 0 12 * * ?` | Daily at noon |
| `0 0 0 1 * ?` | First day of month at midnight |
| `0 0 0 ? * MON` | Every Monday at midnight |

## Service Lifecycle

### Startup

1. Services are registered in `Program.cs`
2. Services start automatically when application starts
3. `OnStartAsync` is called for each service
4. Services begin their execution loops

### Execution

1. Services execute `ExecuteWorkAsync` in a loop
2. Delay between executions based on `GetDelayInterval()`
3. Errors are caught and `OnErrorAsync` is called
4. Services continue running until cancellation

### Shutdown

1. Cancellation token is triggered on application shutdown
2. Services complete current work iteration
3. `OnStopAsync` is called for cleanup
4. Services dispose resources

## Best Practices

### 1. Handle Cancellation Properly

```csharp
protected override async Task ExecuteWorkAsync(CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        // Check cancellation frequently
        if (cancellationToken.IsCancellationRequested)
        {
            break;
        }
        
        // Do work
    }
}
```

### 2. Use Scoped Services

```csharp
protected override async Task ExecuteWorkAsync(CancellationToken cancellationToken)
{
    using var scope = ServiceProvider.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<IMyService>();
    // Use service
}
```

### 3. Implement Proper Error Handling

```csharp
protected override async Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
{
    Logger.LogError(exception, "Error in background service");
    // Optionally send alerts, update metrics, etc.
}
```

### 4. Configure Appropriate Intervals

- **High-frequency**: Data polling (30 seconds)
- **Medium-frequency**: Health checks (5 minutes)
- **Low-frequency**: Data sync (10 minutes)
- **Scheduled**: Daily/weekly tasks

### 5. Use DisallowConcurrentExecution

For jobs that shouldn't run concurrently:

```csharp
[DisallowConcurrentExecution]
public class MyJob : ScheduledJobBase
{
    // Job implementation
}
```

## Monitoring

### Service Status

Check service status via health checks:

```http
GET /health
```

### Logging

All background services log their activities:

```
[Information] Data polling service started with interval 00:00:30
[Debug] Starting data polling cycle
[Debug] Polling data for vessel vessel-001
[Information] Data polling cycle completed
```

### Metrics

Background services can expose metrics:

- Execution count
- Execution duration
- Error count
- Queue size (for message processors)

## Troubleshooting

### Service Not Starting

**Problem**: Background service doesn't start

**Solutions**:
- Check service registration in `Program.cs`
- Verify service implements `IHostedService` or inherits from `BackgroundService`
- Check for exceptions in startup logs
- Verify configuration options are valid

### High Resource Usage

**Problem**: Background services consuming too many resources

**Solutions**:
- Increase polling intervals
- Reduce concurrent operations
- Optimize service logic
- Add rate limiting

### Service Crashes

**Problem**: Background service crashes repeatedly

**Solutions**:
- Review error logs
- Add try-catch blocks
- Implement circuit breaker pattern
- Add health checks

### Scheduled Jobs Not Running

**Problem**: Quartz jobs not executing

**Solutions**:
- Verify job registration
- Check trigger configuration
- Verify scheduler is started
- Check job factory configuration

## Configuration Examples

### Development

```json
{
  "BackgroundServices": {
    "DataPolling": {
      "PollingInterval": "00:00:10",
      "MaxConcurrentVessels": 5
    },
    "PeriodicHealthCheck": {
      "CheckInterval": "00:01:00"
    }
  }
}
```

### Production

```json
{
  "BackgroundServices": {
    "DataPolling": {
      "PollingInterval": "00:00:30",
      "MaxConcurrentVessels": 20
    },
    "PeriodicHealthCheck": {
      "CheckInterval": "00:05:00"
    }
  }
}
```

## Related Documentation

- [Architecture Documentation](ARCHITECTURE.md)
- [Health Checks Documentation](MONITORING.md)
- [Configuration Guide](CONFIGURATION.md)

