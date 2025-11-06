using System.Runtime.Serialization;

namespace KChief.Platform.Core.Exceptions;

/// <summary>
/// Exception thrown when a protocol operation fails (OPC UA, Modbus, etc.).
/// </summary>
[Serializable]
public class ProtocolException : KChiefException
{
    public override string ErrorCode => "PROTOCOL_ERROR";

    public string Protocol { get; }
    public string? Endpoint { get; }

    public ProtocolException(string protocol, string message) 
        : base($"Protocol '{protocol}' error: {message}")
    {
        Protocol = protocol;
        WithContext("Protocol", protocol);
    }

    public ProtocolException(string protocol, string endpoint, string message) 
        : base($"Protocol '{protocol}' error on endpoint '{endpoint}': {message}")
    {
        Protocol = protocol;
        Endpoint = endpoint;
        WithContext("Protocol", protocol)
            .WithContext("Endpoint", endpoint);
    }

    public ProtocolException(string protocol, string message, Exception innerException) 
        : base($"Protocol '{protocol}' error: {message}", innerException)
    {
        Protocol = protocol;
        WithContext("Protocol", protocol);
    }

    public ProtocolException(string protocol, string endpoint, string message, Exception innerException) 
        : base($"Protocol '{protocol}' error on endpoint '{endpoint}': {message}", innerException)
    {
        Protocol = protocol;
        Endpoint = endpoint;
        WithContext("Protocol", protocol)
            .WithContext("Endpoint", endpoint);
    }

    protected ProtocolException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
        Protocol = info.GetString(nameof(Protocol)) ?? string.Empty;
        Endpoint = info.GetString(nameof(Endpoint));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Protocol), Protocol);
        info.AddValue(nameof(Endpoint), Endpoint);
    }
}
