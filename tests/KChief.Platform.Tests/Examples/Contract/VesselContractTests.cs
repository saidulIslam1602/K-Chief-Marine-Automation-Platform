using System.Text.Json;
using KChief.Platform.Core.Models;
using KChief.Platform.Tests.TestHelpers.Contract;
using Xunit;

namespace KChief.Platform.Tests.Examples.Contract;

/// <summary>
/// Example contract tests for Vessel API.
/// </summary>
public class VesselContractTests : ContractTestBase
{
    [Fact]
    public void Vessel_Contract_Should_Have_Required_Properties()
    {
        var vessel = new Vessel
        {
            Id = "vessel-001",
            Name = "Test Vessel",
            IMONumber = "IMO1234567",
            CallSign = "TEST",
            Type = VesselType.Cargo,
            Length = 100.0,
            Width = 20.0,
            Draft = 5.0
        };

        AssertContractHasRequiredProperties(vessel, 
            "id", "name", "imoNumber", "callSign", "type", "length", "width", "draft");
    }

    [Fact]
    public void Vessel_Contract_Should_Serialize_And_Deserialize_Correctly()
    {
        var original = new Vessel
        {
            Id = "vessel-001",
            Name = "Test Vessel",
            IMONumber = "IMO1234567",
            CallSign = "TEST",
            Type = VesselType.Cargo,
            Length = 100.0,
            Width = 20.0,
            Draft = 5.0,
            GrossTonnage = 5000.0,
            Flag = "US",
            BuiltDate = DateTime.UtcNow.AddYears(-10),
            Owner = "Test Owner",
            Status = VesselStatus.InService
        };

        AssertContractRoundTrip(original);
    }

    [Fact]
    public void Vessel_Contract_Should_Match_Expected_Schema()
    {
        var actual = new Vessel
        {
            Id = "vessel-001",
            Name = "Test Vessel",
            IMONumber = "IMO1234567",
            CallSign = "TEST",
            Type = VesselType.Cargo,
            Length = 100.0,
            Width = 20.0,
            Draft = 5.0
        };

        var expected = new Vessel
        {
            Id = "vessel-001",
            Name = "Test Vessel",
            IMONumber = "IMO1234567",
            CallSign = "TEST",
            Type = VesselType.Cargo,
            Length = 100.0,
            Width = 20.0,
            Draft = 5.0
        };

        AssertContractMatches(actual, expected, "Vessel");
    }
}

