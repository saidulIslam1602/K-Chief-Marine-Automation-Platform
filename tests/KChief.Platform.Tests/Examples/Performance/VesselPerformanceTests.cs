using KChief.Platform.Tests.TestHelpers.Performance;
using Xunit;

namespace KChief.Platform.Tests.Examples.Performance;

/// <summary>
/// Example performance tests for vessel operations.
/// </summary>
public class VesselPerformanceTests : PerformanceTestBase
{
    [Fact]
    public void CreateVessel_Should_Complete_Within_100ms()
    {
        AssertExecutionTime(() =>
        {
            // Simulate vessel creation
            var vessel = new KChief.Platform.Core.Models.Vessel
            {
                Id = "vessel-001",
                Name = "Test Vessel",
                Length = 100.0,
                Width = 20.0
            };
        }, TimeSpan.FromMilliseconds(100), "CreateVessel");
    }

    [Fact]
    public void ProcessVessels_Should_Handle_1000_Items_Within_5Seconds()
    {
        var vessels = Enumerable.Range(1, 1000)
            .Select(i => new KChief.Platform.Core.Models.Vessel
            {
                Id = $"vessel-{i:D3}",
                Name = $"Vessel {i}",
                Length = 100.0 + i
            })
            .ToList();

        RunLoadTest(() =>
        {
            // Simulate processing
            var count = vessels.Count;
            var totalLength = vessels.Sum(v => v.Length);
        }, iterations: 100, maxTotalDuration: TimeSpan.FromSeconds(5), "ProcessVessels");
    }

    [Fact]
    public async Task AsyncOperation_Should_Complete_Within_500ms()
    {
        await AssertExecutionTimeAsync(async () =>
        {
            await Task.Delay(10); // Simulate async work
        }, TimeSpan.FromMilliseconds(500), "AsyncOperation");
    }
}

