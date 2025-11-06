using System.Runtime.Serialization;

namespace KChief.Platform.Core.Exceptions;

/// <summary>
/// Exception thrown when a requested vessel is not found.
/// </summary>
[Serializable]
public class VesselNotFoundException : KChiefException
{
    public override string ErrorCode => "VESSEL_NOT_FOUND";

    public string VesselId { get; }

    public VesselNotFoundException(string vesselId) 
        : base($"Vessel with ID '{vesselId}' was not found.")
    {
        VesselId = vesselId;
        WithContext("VesselId", vesselId);
    }

    public VesselNotFoundException(string vesselId, string message) 
        : base(message)
    {
        VesselId = vesselId;
        WithContext("VesselId", vesselId);
    }

    public VesselNotFoundException(string vesselId, string message, Exception innerException) 
        : base(message, innerException)
    {
        VesselId = vesselId;
        WithContext("VesselId", vesselId);
    }

    protected VesselNotFoundException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
        VesselId = info.GetString(nameof(VesselId)) ?? string.Empty;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(VesselId), VesselId);
    }
}
