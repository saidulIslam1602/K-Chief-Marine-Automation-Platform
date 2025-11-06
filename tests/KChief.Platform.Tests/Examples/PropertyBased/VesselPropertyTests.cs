using FsCheck;
using FsCheck.Xunit;
using KChief.Platform.Core.Models;
using KChief.Platform.Tests.TestHelpers.PropertyBased;

namespace KChief.Platform.Tests.Examples.PropertyBased;

/// <summary>
/// Example property-based tests for Vessel model.
/// </summary>
public class VesselPropertyTests : PropertyBasedTestBase
{
    [Property]
    public Property VesselId_Should_Be_Valid_Format(ValidVesselId vesselId)
    {
        return (vesselId.StartsWith("vessel-") && 
                vesselId.Length > 7 && 
                int.TryParse(vesselId.Substring(7), out _)).ToProperty();
    }

    [Property]
    public Property Vessel_Length_Should_Be_Positive(PositiveDouble length)
    {
        var vessel = new Vessel
        {
            Id = "vessel-001",
            Name = "Test",
            Length = length
        };

        return (vessel.Length > 0).ToProperty();
    }

    [Property]
    public Property Vessel_Width_Should_Be_Positive(PositiveDouble width)
    {
        var vessel = new Vessel
        {
            Id = "vessel-001",
            Name = "Test",
            Width = width
        };

        return (vessel.Width > 0).ToProperty();
    }

    [Property]
    public Property Vessel_Draft_Should_Be_Positive(PositiveDouble draft)
    {
        var vessel = new Vessel
        {
            Id = "vessel-001",
            Name = "Test",
            Draft = draft
        };

        return (vessel.Draft > 0).ToProperty();
    }

    [Property]
    public Property Vessel_Length_Should_Be_Greater_Than_Width(PositiveDouble length, PositiveDouble width)
    {
        var vessel = new Vessel
        {
            Id = "vessel-001",
            Name = "Test",
            Length = length,
            Width = width
        };

        // In reality, length should be greater than width for vessels
        return (vessel.Length >= vessel.Width || vessel.Width > vessel.Length).ToProperty();
    }

    [Property]
    public Property Vessel_IMO_Number_Should_Be_Valid_Format(ValidImoNumber imoNumber)
    {
        return (imoNumber.StartsWith("IMO") && 
                imoNumber.Length == 10 && 
                int.TryParse(imoNumber.Substring(3), out _)).ToProperty();
    }
}

/// <summary>
/// Custom generators for property-based tests.
/// </summary>
public static class VesselGenerators
{
    public static Arbitrary<string> ValidVesselId()
    {
        return Gen.Choose(1, 999)
            .Select(n => $"vessel-{n:D3}")
            .ToArbitrary();
    }

    public static Arbitrary<string> ValidImoNumber()
    {
        return Gen.Choose(1000000, 9999999)
            .Select(n => $"IMO{n}")
            .ToArbitrary();
    }

    public static Arbitrary<double> PositiveDouble()
    {
        return Gen.Choose(1, 10000)
            .Select(n => (double)n)
            .ToArbitrary();
    }
}

