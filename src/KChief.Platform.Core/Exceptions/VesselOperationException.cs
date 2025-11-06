using System.Runtime.Serialization;

namespace KChief.Platform.Core.Exceptions;

/// <summary>
/// Exception thrown when a vessel operation fails.
/// </summary>
[Serializable]
public class VesselOperationException : KChiefException
{
    public override string ErrorCode => "VESSEL_OPERATION_FAILED";

    public string VesselId { get; }
    public string Operation { get; }

    public VesselOperationException(string vesselId, string operation) 
        : base($"Operation '{operation}' failed on vessel '{vesselId}'.")
    {
        VesselId = vesselId;
        Operation = operation;
        WithContext("VesselId", vesselId)
            .WithContext("Operation", operation);
    }

    public VesselOperationException(string vesselId, string operation, string message) 
        : base(message)
    {
        VesselId = vesselId;
        Operation = operation;
        WithContext("VesselId", vesselId)
            .WithContext("Operation", operation);
    }

    public VesselOperationException(string vesselId, string operation, string message, Exception innerException) 
        : base(message, innerException)
    {
        VesselId = vesselId;
        Operation = operation;
        WithContext("VesselId", vesselId)
            .WithContext("Operation", operation);
    }

    protected VesselOperationException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
        VesselId = info.GetString(nameof(VesselId)) ?? string.Empty;
        Operation = info.GetString(nameof(Operation)) ?? string.Empty;
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(VesselId), VesselId);
        info.AddValue(nameof(Operation), Operation);
    }
}
