using KChief.Platform.Core.ConnectionPooling;
using NModbus;
using System.Net.Sockets;

namespace KChief.Protocols.Modbus.Services;

/// <summary>
/// Connection pool for Modbus TCP connections.
/// </summary>
public class ModbusConnectionPool : BaseConnectionPool<ModbusConnection>
{
    private readonly string _ipAddress;
    private readonly int _port;

    public ModbusConnectionPool(string ipAddress, int port = 502, ConnectionPoolOptions? options = null)
        : base(options)
    {
        _ipAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
        _port = port;
    }

    protected override async Task<ModbusConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var tcpClient = new TcpClient();
        
        try
        {
            await tcpClient.ConnectAsync(_ipAddress, _port);
            
            if (!tcpClient.Connected)
            {
                throw new InvalidOperationException("Failed to connect to Modbus server");
            }

            var factory = new ModbusFactory();
            var master = factory.CreateMaster(tcpClient);

            return new ModbusConnection
            {
                TcpClient = tcpClient,
                Master = master
            };
        }
        catch
        {
            tcpClient?.Dispose();
            throw;
        }
    }

    protected override async Task<bool> IsConnectionValidAsync(ModbusConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            return connection?.TcpClient?.Connected ?? false;
        }
        catch
        {
            return false;
        }
    }

    protected override async Task DisposeConnectionAsync(ModbusConnection connection)
    {
        try
        {
            if (connection?.Master is IDisposable disposable)
            {
                disposable.Dispose();
            }

            connection?.TcpClient?.Close();
            connection?.TcpClient?.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }

        await Task.CompletedTask;
    }
}

/// <summary>
/// Wrapper for Modbus connection.
/// </summary>
public class ModbusConnection
{
    public TcpClient? TcpClient { get; set; }
    public IModbusMaster? Master { get; set; }
}

