using System.Runtime.Serialization;

namespace KChief.Platform.Core.Exceptions;

/// <summary>
/// Exception thrown when a requested engine is not found.
/// </summary>
[Serializable]
public class EngineNotFoundException : KChiefException
{
    public override string ErrorCode => "ENGINE_NOT_FOUND";

    public string EngineId { get; }
    public string? VesselId { get; }

    public EngineNotFoundException(string engineId) 
        : base($"Engine with ID '{engineId}' was not found.")
    {
        EngineId = engineId;
        WithContext("EngineId", engineId);
    }

    public EngineNotFoundException(string engineId, string vesselId) 
        : base($"Engine with ID '{engineId}' was not found on vessel '{vesselId}'.")
    {
        EngineId = engineId;
        VesselId = vesselId;
        WithContext("EngineId", engineId)
            .WithContext("VesselId", vesselId);
    }

    public EngineNotFoundException(string engineId, string message, Exception innerException) 
        : base(message, innerException)
    {
        EngineId = engineId;
        WithContext("EngineId", engineId);
    }

    protected EngineNotFoundException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
        EngineId = info.GetString(nameof(EngineId)) ?? string.Empty;
        VesselId = info.GetString(nameof(VesselId));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(EngineId), EngineId);
        info.AddValue(nameof(VesselId), VesselId);
    }
}
