using KChief.Platform.Core.Models;

namespace KChief.Platform.Tests.TestHelpers.Builders;

/// <summary>
/// Builder for creating test vessel instances.
/// </summary>
public class VesselBuilder
{
    private string _id = "vessel-001";
    private string _name = "Test Vessel";
    private string _imoNumber = "IMO1234567";
    private string _callSign = "TEST";
    private VesselType _type = VesselType.Cargo;
    private double _length = 100.0;
    private double _width = 20.0;
    private double _draft = 5.0;
    private double _grossTonnage = 5000.0;
    private string? _flag = "US";
    private DateTime _builtDate = DateTime.UtcNow.AddYears(-10);
    private string? _owner = "Test Owner";
    private VesselStatus _status = VesselStatus.InService;

    public VesselBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public VesselBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public VesselBuilder WithImoNumber(string imoNumber)
    {
        _imoNumber = imoNumber;
        return this;
    }

    public VesselBuilder WithCallSign(string callSign)
    {
        _callSign = callSign;
        return this;
    }

    public VesselBuilder WithType(VesselType type)
    {
        _type = type;
        return this;
    }

    public VesselBuilder WithDimensions(double length, double width, double draft)
    {
        _length = length;
        _width = width;
        _draft = draft;
        return this;
    }

    public VesselBuilder WithGrossTonnage(double grossTonnage)
    {
        _grossTonnage = grossTonnage;
        return this;
    }

    public VesselBuilder WithFlag(string flag)
    {
        _flag = flag;
        return this;
    }

    public VesselBuilder WithBuiltDate(DateTime builtDate)
    {
        _builtDate = builtDate;
        return this;
    }

    public VesselBuilder WithOwner(string owner)
    {
        _owner = owner;
        return this;
    }

    public VesselBuilder WithStatus(VesselStatus status)
    {
        _status = status;
        return this;
    }

    public VesselBuilder AsCargoVessel()
    {
        _type = VesselType.Cargo;
        return this;
    }

    public VesselBuilder AsTanker()
    {
        _type = VesselType.Tanker;
        return this;
    }

    public VesselBuilder AsContainerShip()
    {
        _type = VesselType.ContainerShip;
        return this;
    }

    public VesselBuilder InService()
    {
        _status = VesselStatus.InService;
        return this;
    }

    public VesselBuilder OutOfService()
    {
        _status = VesselStatus.OutOfService;
        return this;
    }

    public Vessel Build()
    {
        return new Vessel
        {
            Id = _id,
            Name = _name,
            IMONumber = _imoNumber,
            CallSign = _callSign,
            Type = _type,
            Length = _length,
            Width = _width,
            Draft = _draft,
            GrossTonnage = _grossTonnage,
            Flag = _flag,
            BuiltDate = _builtDate,
            Owner = _owner,
            Status = _status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static VesselBuilder Create() => new();

    public static VesselBuilder CreateDefault() => new()
        .WithId("vessel-001")
        .WithName("Test Vessel")
        .WithImoNumber("IMO1234567")
        .WithCallSign("TEST")
        .AsCargoVessel()
        .InService();
}

