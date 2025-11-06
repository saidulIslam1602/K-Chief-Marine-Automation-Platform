using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace KChief.Platform.Tests.TestHelpers.PropertyBased;

/// <summary>
/// Base class for property-based tests.
/// </summary>
public abstract class PropertyBasedTestBase
{
    protected static Gen<string> ValidVesselId => 
        Gen.Choose(1, 999).Select(n => $"vessel-{n:D3}");

    protected static Gen<string> ValidEngineId => 
        Gen.Choose(1, 999).Select(n => $"engine-{n:D3}");

    protected static Gen<string> ValidSensorId => 
        Gen.Choose(1, 999).Select(n => $"sensor-{n:D3}");

    protected static Gen<string> ValidImoNumber => 
        Gen.Choose(1000000, 9999999).Select(n => $"IMO{n}");

    protected static Gen<string> ValidCallSign => 
        Gen.Choose(4, 8)
            .SelectMany(length => 
                Gen.ArrayOf(length, Gen.Choose('A', 'Z'))
                    .Select(chars => new string(chars)));

    protected static Gen<double> PositiveDouble => 
        Gen.Choose(1, 10000).Select(n => (double)n);

    protected static Gen<double> ValidRpm => 
        Gen.Choose(0, 2000).Select(n => (double)n);

    protected static Gen<double> ValidTemperature => 
        Gen.Choose(-50, 200).Select(n => (double)n);

    protected static Gen<DateTime> ValidDate => 
        Gen.Choose(1900, DateTime.UtcNow.Year)
            .SelectMany(year => 
                Gen.Choose(1, 12).SelectMany(month =>
                    Gen.Choose(1, DateTime.DaysInMonth(year, month))
                        .Select(day => new DateTime(year, month, day))));

    protected static Gen<VesselType> VesselTypeGen => 
        Gen.Elements(VesselType.Cargo, VesselType.Tanker, VesselType.ContainerShip, VesselType.Passenger);

    protected static Gen<EngineType> EngineTypeGen => 
        Gen.Elements(EngineType.Diesel, EngineType.GasTurbine, EngineType.Electric);

    protected static Gen<AlarmSeverity> AlarmSeverityGen => 
        Gen.Elements(AlarmSeverity.Info, AlarmSeverity.Warning, AlarmSeverity.Critical);
}

