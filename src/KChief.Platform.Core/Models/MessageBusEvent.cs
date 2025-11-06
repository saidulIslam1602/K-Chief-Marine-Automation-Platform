namespace KChief.Platform.Core.Models;

/// <summary>
/// Base class for message bus events.
/// </summary>
public class MessageBusEvent
{
    /// <summary>
    /// Event ID.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Event type name.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Source of the event.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Event data as JSON string.
    /// </summary>
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Event published when a vessel status changes.
/// </summary>
public class VesselStatusChangedEvent : MessageBusEvent
{
    public string VesselId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public VesselStatusChangedEvent()
    {
        EventType = nameof(VesselStatusChangedEvent);
    }
}

/// <summary>
/// Event published when an engine status changes.
/// </summary>
public class EngineStatusChangedEvent : MessageBusEvent
{
    public string VesselId { get; set; } = string.Empty;
    public string EngineId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Rpm { get; set; }

    public EngineStatusChangedEvent()
    {
        EventType = nameof(EngineStatusChangedEvent);
    }
}

/// <summary>
/// Event published when an alarm is created.
/// </summary>
public class AlarmCreatedEvent : MessageBusEvent
{
    public string AlarmId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? VesselId { get; set; }

    public AlarmCreatedEvent()
    {
        EventType = nameof(AlarmCreatedEvent);
    }
}

