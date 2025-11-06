using System.Diagnostics;
using Xunit;

namespace HMI.Platform.Tests.TestHelpers.Performance;

/// <summary>
/// Base class for performance tests.
/// </summary>
public abstract class PerformanceTestBase
{
    /// <summary>
    /// Asserts that an operation completes within the specified time.
    /// </summary>
    protected void AssertExecutionTime(Action action, TimeSpan maxDuration, string operationName = "Operation")
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();

        Assert.True(
            stopwatch.Elapsed <= maxDuration,
            $"{operationName} took {stopwatch.Elapsed.TotalMilliseconds}ms, expected <= {maxDuration.TotalMilliseconds}ms");
    }

    /// <summary>
    /// Asserts that an async operation completes within the specified time.
    /// </summary>
    protected async Task AssertExecutionTimeAsync(Func<Task> action, TimeSpan maxDuration, string operationName = "Operation")
    {
        var stopwatch = Stopwatch.StartNew();
        await action();
        stopwatch.Stop();

        Assert.True(
            stopwatch.Elapsed <= maxDuration,
            $"{operationName} took {stopwatch.Elapsed.TotalMilliseconds}ms, expected <= {maxDuration.TotalMilliseconds}ms");
    }

    /// <summary>
    /// Measures execution time and returns the duration.
    /// </summary>
    protected TimeSpan MeasureExecutionTime(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    /// <summary>
    /// Measures execution time of an async operation and returns the duration.
    /// </summary>
    protected async Task<TimeSpan> MeasureExecutionTimeAsync(Func<Task> action)
    {
        var stopwatch = Stopwatch.StartNew();
        await action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    /// <summary>
    /// Runs a load test with multiple iterations.
    /// </summary>
    protected void RunLoadTest(Action action, int iterations, TimeSpan maxTotalDuration, string testName = "LoadTest")
    {
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            action();
        }
        
        stopwatch.Stop();
        var averageDuration = TimeSpan.FromMilliseconds(stopwatch.Elapsed.TotalMilliseconds / iterations);

        Assert.True(
            stopwatch.Elapsed <= maxTotalDuration,
            $"{testName} completed {iterations} iterations in {stopwatch.Elapsed.TotalMilliseconds}ms " +
            $"(avg: {averageDuration.TotalMilliseconds}ms/iteration), expected <= {maxTotalDuration.TotalMilliseconds}ms");
    }

    /// <summary>
    /// Runs a load test with multiple async iterations.
    /// </summary>
    protected async Task RunLoadTestAsync(Func<Task> action, int iterations, TimeSpan maxTotalDuration, string testName = "LoadTest")
    {
        var stopwatch = Stopwatch.StartNew();
        
        var tasks = new List<Task>();
        for (int i = 0; i < iterations; i++)
        {
            tasks.Add(action());
        }
        
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        var averageDuration = TimeSpan.FromMilliseconds(stopwatch.Elapsed.TotalMilliseconds / iterations);

        Assert.True(
            stopwatch.Elapsed <= maxTotalDuration,
            $"{testName} completed {iterations} iterations in {stopwatch.Elapsed.TotalMilliseconds}ms " +
            $"(avg: {averageDuration.TotalMilliseconds}ms/iteration), expected <= {maxTotalDuration.TotalMilliseconds}ms");
    }
}

