using HMI.Platform.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace HMI.DataAccess.Services;

/// <summary>
/// RabbitMQ message bus service implementation.
/// For production, this would connect to a real RabbitMQ server.
/// This is a simplified implementation for demonstration.
/// </summary>
public class MessageBusService : IMessageBus, IDisposable
{
    private readonly ILogger<MessageBusService> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly ConnectionFactory _connectionFactory;
    private bool _disposed = false;

    public bool IsConnected => _connection?.IsOpen ?? false;

    public MessageBusService(ILogger<MessageBusService> logger)
    {
        _logger = logger;
        _connectionFactory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };
    }

    public async Task ConnectAsync()
    {
        try
        {
            if (!IsConnected)
            {
                _connection = await Task.FromResult(_connectionFactory.CreateConnection());
                _channel = _connection.CreateModel();
                _logger.LogInformation("Connected to message bus");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to message bus. Running in simulation mode.");
            // In simulation mode, we continue without actual connection
        }
    }

    public async Task DisconnectAsync()
    {
        if (_channel != null && _channel.IsOpen)
        {
            _channel.Close();
            _channel.Dispose();
            _channel = null;
        }

        if (_connection != null && _connection.IsOpen)
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }

        await Task.CompletedTask;
    }

    public async Task PublishAsync<T>(string queueName, T message) where T : class
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Message bus not connected. Message not published: {QueueName}", queueName);
            return;
        }

        try
        {
            _channel?.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel?.CreateBasicProperties();
            if (properties != null)
            {
                properties.Persistent = true;
            }

            _channel?.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);

            _logger.LogInformation("Published message to queue: {QueueName}", queueName);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to queue: {QueueName}", queueName);
        }
    }

    public async Task SubscribeAsync<T>(string queueName, Func<T, Task> handler) where T : class
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Message bus not connected. Cannot subscribe to: {QueueName}", queueName);
            return;
        }

        try
        {
            _channel?.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var deserializedMessage = JsonSerializer.Deserialize<T>(message);

                    if (deserializedMessage != null)
                    {
                        await handler(deserializedMessage);
                        _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue: {QueueName}", queueName);
                    _channel?.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel?.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("Subscribed to queue: {QueueName}", queueName);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to queue: {QueueName}", queueName);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            DisconnectAsync().GetAwaiter().GetResult();
            _disposed = true;
        }
    }
}

