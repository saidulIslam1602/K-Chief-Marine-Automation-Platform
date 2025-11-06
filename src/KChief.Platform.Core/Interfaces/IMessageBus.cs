namespace KChief.Platform.Core.Interfaces;

/// <summary>
/// Interface for message bus operations.
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publishes a message to a queue.
    /// </summary>
    Task PublishAsync<T>(string queueName, T message) where T : class;

    /// <summary>
    /// Subscribes to messages from a queue.
    /// </summary>
    Task SubscribeAsync<T>(string queueName, Func<T, Task> handler) where T : class;

    /// <summary>
    /// Connects to the message bus.
    /// </summary>
    Task ConnectAsync();

    /// <summary>
    /// Disconnects from the message bus.
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Checks if the message bus is connected.
    /// </summary>
    bool IsConnected { get; }
}

