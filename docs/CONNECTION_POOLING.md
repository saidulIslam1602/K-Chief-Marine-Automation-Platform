# Connection Pooling and Resource Management

## Overview

The K-Chief Marine Automation Platform implements comprehensive connection pooling and resource management for protocol connections (OPC UA, Modbus) to optimize performance, reduce connection overhead, and ensure proper resource disposal.

## Architecture

### Connection Pool Components

```
Connection Pooling System
├── IConnectionPool<T>          # Pool interface
├── BaseConnectionPool<T>       # Base implementation
├── ConnectionPoolOptions       # Configuration
├── PooledConnection<T>         # Wrapped connection
├── ConnectionRetryPolicy       # Retry logic
└── ConnectionHealthMonitor     # Health monitoring
```

## Connection Pooling

### Base Connection Pool

The `BaseConnectionPool<T>` provides:

- **Connection Acquisition**: Get connections from pool with timeout
- **Connection Return**: Return connections to pool
- **Health Checking**: Periodic validation of connections
- **Cleanup**: Remove idle and expired connections
- **Size Management**: Maintain min/max pool sizes

### Usage Example

```csharp
// Create pool
var poolOptions = new ConnectionPoolOptions
{
    MaxSize = 10,
    MinSize = 2,
    AcquireTimeout = TimeSpan.FromSeconds(30),
    MaxLifetime = TimeSpan.FromHours(1),
    MaxIdleTime = TimeSpan.FromMinutes(30),
    HealthCheckInterval = TimeSpan.FromMinutes(5)
};

var pool = new OPCUaConnectionPool("opc.tcp://server:4840", poolOptions);

// Acquire connection
using (var pooledConnection = await pool.AcquireAsync())
{
    var session = pooledConnection.Connection;
    // Use connection
    var value = await session.ReadNodeValueAsync("ns=2;s=Temperature");
}

// Connection is automatically returned to pool
```

### OPC UA Connection Pool

```csharp
var opcPool = new OPCUaConnectionPool(
    endpointUrl: "opc.tcp://server:4840",
    options: new ConnectionPoolOptions
    {
        MaxSize = 10,
        MinSize = 2,
        ValidateOnAcquire = true,
        ValidateOnReturn = true
    });

// Use pooled connection
using (var connection = await opcPool.AcquireAsync())
{
    var session = connection.Connection;
    // Perform operations
}
```

### Modbus Connection Pool

```csharp
var modbusPool = new ModbusConnectionPool(
    ipAddress: "192.168.1.100",
    port: 502,
    options: new ConnectionPoolOptions
    {
        MaxSize = 5,
        MinSize = 1
    });

// Use pooled connection
using (var connection = await modbusPool.AcquireAsync())
{
    var master = connection.Connection.Master;
    var registers = await master.ReadHoldingRegistersAsync(1, 0, 10);
}
```

## Connection Pool Options

### Configuration

```csharp
var options = new ConnectionPoolOptions
{
    // Pool size
    MaxSize = 10,                    // Maximum connections
    MinSize = 2,                     // Minimum connections to maintain

    // Timeouts
    AcquireTimeout = TimeSpan.FromSeconds(30),  // Wait time for connection
    ConnectionTimeout = TimeSpan.FromSeconds(10), // Connection creation timeout

    // Lifetime management
    MaxLifetime = TimeSpan.FromHours(1),       // Max connection age
    MaxIdleTime = TimeSpan.FromMinutes(30),     // Max idle time

    // Health checking
    HealthCheckInterval = TimeSpan.FromMinutes(5),
    ValidateOnAcquire = true,                   // Validate before use
    ValidateOnReturn = true                     // Validate before return
};
```

### Options Explained

| Option | Description | Default |
|--------|-------------|---------|
| `MaxSize` | Maximum connections in pool | 10 |
| `MinSize` | Minimum connections to maintain | 2 |
| `AcquireTimeout` | Max wait time for connection | 30s |
| `ConnectionTimeout` | Connection creation timeout | 10s |
| `MaxLifetime` | Max connection age before replacement | 1 hour |
| `MaxIdleTime` | Max idle time before removal | 30 minutes |
| `HealthCheckInterval` | Interval for health checks | 5 minutes |
| `ValidateOnAcquire` | Validate connection before use | true |
| `ValidateOnReturn` | Validate connection before return | true |

## Resource Management

### Async Disposable Pattern

```csharp
public class MyResource : AsyncDisposableBase
{
    protected override void DisposeManagedResources()
    {
        // Dispose managed resources
        _connection?.Dispose();
    }

    protected override void DisposeUnmanagedResources()
    {
        // Dispose unmanaged resources
        // Close handles, etc.
    }
}

// Usage
await using (var resource = new MyResource())
{
    // Use resource
}
// Automatically disposed
```

### Resource Manager

```csharp
var resourceManager = new ResourceManager();

// Register resources
resourceManager.Register(connectionPool);
resourceManager.Register(timer);
resourceManager.Register(service);

// Dispose all at once
await resourceManager.DisposeAsync();
```

## Connection Retry Logic

### Retry Policy

```csharp
var retryPolicy = new ConnectionRetryPolicy(
    maxRetries: 3,
    delay: TimeSpan.FromSeconds(1));

// Execute with retry
await retryPolicy.ExecuteAsync(async () =>
{
    await connection.ConnectAsync();
});
```

### Retryable Exceptions

The retry policy automatically retries on:
- `TimeoutException`
- `SocketException`
- `IOException`
- `AggregateException` containing retryable exceptions

### Custom Retry Logic

```csharp
var policy = Policy
    .Handle<TimeoutException>()
    .Or<SocketException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            _logger.LogWarning(
                "Retry {RetryCount} after {Delay}ms",
                retryCount, timespan.TotalMilliseconds);
        });

await policy.ExecuteAsync(async () =>
{
    await connection.ConnectAsync();
});
```

## Connection Health Monitoring

### Health Monitor

```csharp
var healthMonitor = new ConnectionHealthMonitor<Session>(
    monitoringInterval: TimeSpan.FromMinutes(1));

// Record events
healthMonitor.RecordAcquisition(connectionId, duration);
healthMonitor.RecordReturn(connectionId, duration);
healthMonitor.RecordError(connectionId, exception);

// Get statistics
var stats = healthMonitor.GetStats(connectionId);
Console.WriteLine($"Error Rate: {stats.ErrorRate:P2}");
Console.WriteLine($"Avg Acquisition Time: {stats.AverageAcquisitionTime}ms");
```

### Health Statistics

```csharp
public class ConnectionHealthStats
{
    public int TotalAcquisitions { get; set; }
    public int TotalReturns { get; set; }
    public int TotalErrors { get; set; }
    public TimeSpan TotalAcquisitionTime { get; set; }
    public TimeSpan TotalUsageTime { get; set; }
    public double AverageAcquisitionTime { get; }
    public double AverageUsageTime { get; }
    public double ErrorRate { get; }
}
```

## Best Practices

### 1. Always Use Using Statements

```csharp
// Good
using (var connection = await pool.AcquireAsync())
{
    // Use connection
}

// Avoid
var connection = await pool.AcquireAsync();
// ... use connection
// Might forget to return
```

### 2. Configure Pool Size Appropriately

```csharp
// For high-throughput scenarios
var options = new ConnectionPoolOptions
{
    MaxSize = 20,
    MinSize = 5
};

// For low-throughput scenarios
var options = new ConnectionPoolOptions
{
    MaxSize = 5,
    MinSize = 1
};
```

### 3. Enable Validation

```csharp
var options = new ConnectionPoolOptions
{
    ValidateOnAcquire = true,  // Catch invalid connections early
    ValidateOnReturn = true    // Prevent returning bad connections
};
```

### 4. Monitor Connection Health

```csharp
var healthMonitor = new ConnectionHealthMonitor<Session>(
    TimeSpan.FromMinutes(1));

// Check error rates
var stats = healthMonitor.GetStats(connectionId);
if (stats.ErrorRate > 0.1) // 10% error rate
{
    // Alert or take action
}
```

### 5. Use Retry Policies

```csharp
var retryPolicy = new ConnectionRetryPolicy(maxRetries: 3);

await retryPolicy.ExecuteAsync(async () =>
{
    using (var connection = await pool.AcquireAsync())
    {
        // Perform operation
    }
});
```

### 6. Dispose Resources Properly

```csharp
// Use ResourceManager for multiple resources
var resourceManager = new ResourceManager();
resourceManager.Register(pool);
resourceManager.Register(monitor);
resourceManager.Register(timer);

// Dispose all at once
await resourceManager.DisposeAsync();
```

## Performance Considerations

### Pool Size Tuning

- **Too Small**: Connection contention, timeouts
- **Too Large**: Resource waste, memory overhead
- **Optimal**: Based on concurrent operations

### Connection Lifetime

- **Too Short**: Frequent reconnection overhead
- **Too Long**: Stale connections, resource leaks
- **Optimal**: Balance between freshness and overhead

### Health Check Frequency

- **Too Frequent**: Performance impact
- **Too Infrequent**: Bad connections in pool
- **Optimal**: Based on connection stability

## Monitoring and Diagnostics

### Pool Metrics

```csharp
var pool = new OPCUaConnectionPool(...);

// Current pool state
Console.WriteLine($"Pool Size: {pool.CurrentSize}");
Console.WriteLine($"Available: {pool.AvailableCount}");
Console.WriteLine($"Active: {pool.ActiveCount}");
Console.WriteLine($"Max Size: {pool.MaxSize}");
```

### Health Statistics

```csharp
var monitor = new ConnectionHealthMonitor<Session>(...);
var allStats = monitor.GetAllStats();

foreach (var stats in allStats)
{
    Console.WriteLine($"Connection {stats.ConnectionId}:");
    Console.WriteLine($"  Error Rate: {stats.ErrorRate:P2}");
    Console.WriteLine($"  Avg Acquisition: {stats.AverageAcquisitionTime}ms");
    Console.WriteLine($"  Avg Usage: {stats.AverageUsageTime}ms");
}
```

## Troubleshooting

### Connection Timeouts

**Problem**: Frequent timeouts when acquiring connections

**Solutions**:
- Increase `MaxSize`
- Increase `AcquireTimeout`
- Check connection creation performance
- Verify network connectivity

### High Error Rates

**Problem**: High error rate in health statistics

**Solutions**:
- Enable `ValidateOnAcquire` and `ValidateOnReturn`
- Reduce `MaxLifetime` to replace connections more frequently
- Check connection stability
- Review retry policies

### Resource Leaks

**Problem**: Connections not being returned to pool

**Solutions**:
- Always use `using` statements
- Ensure proper exception handling
- Use `ResourceManager` for cleanup
- Monitor `ActiveCount` vs `AvailableCount`

## Related Documentation

- [Architecture Documentation](ARCHITECTURE.md)
- [Protocol Documentation](PROTOCOLS.md)
- [Performance Guide](PERFORMANCE.md)

