using KChief.Platform.Core.Models;
using KChief.Platform.Tests.TestHelpers.Builders;
using Xunit;

namespace KChief.Platform.Tests.Examples.Builders;

/// <summary>
/// Example tests demonstrating the builder pattern.
/// </summary>
public class VesselBuilderTests
{
    [Fact]
    public void VesselBuilder_Should_Create_Default_Vessel()
    {
        var vessel = VesselBuilder.CreateDefault().Build();

        Assert.NotNull(vessel);
        Assert.Equal("vessel-001", vessel.Id);
        Assert.Equal("Test Vessel", vessel.Name);
        Assert.Equal(VesselType.Cargo, vessel.Type);
        Assert.Equal(VesselStatus.InService, vessel.Status);
    }

    [Fact]
    public void VesselBuilder_Should_Create_Custom_Vessel()
    {
        var vessel = VesselBuilder.Create()
            .WithId("vessel-999")
            .WithName("Custom Vessel")
            .WithImoNumber("IMO9999999")
            .AsTanker()
            .OutOfService()
            .WithDimensions(200.0, 30.0, 10.0)
            .Build();

        Assert.Equal("vessel-999", vessel.Id);
        Assert.Equal("Custom Vessel", vessel.Name);
        Assert.Equal("IMO9999999", vessel.IMONumber);
        Assert.Equal(VesselType.Tanker, vessel.Type);
        Assert.Equal(VesselStatus.OutOfService, vessel.Status);
        Assert.Equal(200.0, vessel.Length);
        Assert.Equal(30.0, vessel.Width);
        Assert.Equal(10.0, vessel.Draft);
    }

    [Fact]
    public void EngineBuilder_Should_Create_Default_Engine()
    {
        var engine = EngineBuilder.CreateDefault().Build();

        Assert.NotNull(engine);
        Assert.Equal("engine-001", engine.Id);
        Assert.Equal("vessel-001", engine.VesselId);
        Assert.Equal("Main Engine", engine.Name);
        Assert.Equal(EngineType.Diesel, engine.Type);
        Assert.Equal(EngineStatus.Stopped, engine.Status);
    }

    [Fact]
    public void EngineBuilder_Should_Create_Running_Engine()
    {
        var engine = EngineBuilder.Create()
            .WithId("engine-002")
            .WithVesselId("vessel-001")
            .WithMaxRpm(1500.0)
            .Running()
            .Build();

        Assert.Equal(EngineStatus.Running, engine.Status);
        Assert.True(engine.CurrentRpm > 0);
    }

    [Fact]
    public void AlarmBuilder_Should_Create_Default_Alarm()
    {
        var alarm = AlarmBuilder.CreateDefault().Build();

        Assert.NotNull(alarm);
        Assert.Equal("alarm-001", alarm.Id);
        Assert.Equal("Test Alarm", alarm.Title);
        Assert.Equal(AlarmSeverity.Warning, alarm.Severity);
        Assert.Equal(AlarmStatus.Active, alarm.Status);
    }

    [Fact]
    public void AlarmBuilder_Should_Create_Acknowledged_Alarm()
    {
        var alarm = AlarmBuilder.Create()
            .WithTitle("High Temperature")
            .AsCritical()
            .Acknowledged("operator-001")
            .Build();

        Assert.Equal(AlarmStatus.Acknowledged, alarm.Status);
        Assert.Equal("operator-001", alarm.AcknowledgedBy);
        Assert.NotNull(alarm.AcknowledgedAt);
        Assert.Equal(AlarmSeverity.Critical, alarm.Severity);
    }
}

