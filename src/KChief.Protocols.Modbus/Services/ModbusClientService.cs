using KChief.Platform.Core.Interfaces;
using NModbus;
using System.Net.Sockets;

namespace KChief.Protocols.Modbus.Services;

/// <summary>
/// Modbus TCP client service implementation.
/// This is a simplified implementation for MVP purposes.
/// </summary>
public class ModbusClientService : IModbusClient, IDisposable
{
    private TcpClient? _tcpClient;
    private object? _modbusMaster;
    private bool _disposed = false;

    public bool IsConnected => _tcpClient?.Connected ?? false;

    public async Task<bool> ConnectAsync(string ipAddress, int port = 502)
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(ipAddress, port);
            
            if (_tcpClient.Connected)
            {
                var factory = new ModbusFactory();
                _modbusMaster = factory.CreateMaster(_tcpClient);
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_modbusMaster is IDisposable disposable)
        {
            disposable.Dispose();
            _modbusMaster = null;
        }

        if (_tcpClient != null)
        {
            _tcpClient.Close();
            _tcpClient.Dispose();
            _tcpClient = null;
        }

        await Task.CompletedTask;
    }

    public async Task<ushort[]?> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort numberOfRegisters)
    {
        if (_modbusMaster == null || !IsConnected)
        {
            throw new InvalidOperationException("Modbus client is not connected.");
        }

        try
        {
            // Using reflection for NModbus 3.0 compatibility
            var method = _modbusMaster.GetType().GetMethod("ReadHoldingRegisters");
            if (method != null)
            {
                var result = method.Invoke(_modbusMaster, new object[] { slaveId, startAddress, numberOfRegisters });
                return await Task.FromResult(result as ushort[]);
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<ushort[]?> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort numberOfRegisters)
    {
        if (_modbusMaster == null || !IsConnected)
        {
            throw new InvalidOperationException("Modbus client is not connected.");
        }

        try
        {
            var method = _modbusMaster.GetType().GetMethod("ReadInputRegisters");
            if (method != null)
            {
                var result = method.Invoke(_modbusMaster, new object[] { slaveId, startAddress, numberOfRegisters });
                return await Task.FromResult(result as ushort[]);
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool[]?> ReadDiscreteInputsAsync(byte slaveId, ushort startAddress, ushort numberOfInputs)
    {
        if (_modbusMaster == null || !IsConnected)
        {
            throw new InvalidOperationException("Modbus client is not connected.");
        }

        try
        {
            var method = _modbusMaster.GetType().GetMethod("ReadInputs");
            if (method != null)
            {
                var result = method.Invoke(_modbusMaster, new object[] { slaveId, startAddress, numberOfInputs });
                return await Task.FromResult(result as bool[]);
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool[]?> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort numberOfCoils)
    {
        if (_modbusMaster == null || !IsConnected)
        {
            throw new InvalidOperationException("Modbus client is not connected.");
        }

        try
        {
            var method = _modbusMaster.GetType().GetMethod("ReadCoils");
            if (method != null)
            {
                var result = method.Invoke(_modbusMaster, new object[] { slaveId, startAddress, numberOfCoils });
                return await Task.FromResult(result as bool[]);
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> WriteSingleRegisterAsync(byte slaveId, ushort registerAddress, ushort value)
    {
        if (_modbusMaster == null || !IsConnected)
        {
            throw new InvalidOperationException("Modbus client is not connected.");
        }

        try
        {
            var method = _modbusMaster.GetType().GetMethod("WriteSingleRegister");
            method?.Invoke(_modbusMaster, new object[] { slaveId, registerAddress, value });
            await Task.CompletedTask;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> WriteMultipleRegistersAsync(byte slaveId, ushort startAddress, ushort[] values)
    {
        if (_modbusMaster == null || !IsConnected)
        {
            throw new InvalidOperationException("Modbus client is not connected.");
        }

        try
        {
            var method = _modbusMaster.GetType().GetMethod("WriteMultipleRegisters");
            method?.Invoke(_modbusMaster, new object[] { slaveId, startAddress, values });
            await Task.CompletedTask;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> WriteSingleCoilAsync(byte slaveId, ushort coilAddress, bool value)
    {
        if (_modbusMaster == null || !IsConnected)
        {
            throw new InvalidOperationException("Modbus client is not connected.");
        }

        try
        {
            var method = _modbusMaster.GetType().GetMethod("WriteSingleCoil");
            method?.Invoke(_modbusMaster, new object[] { slaveId, coilAddress, value });
            await Task.CompletedTask;
            return true;
        }
        catch (Exception)
        {
            return false;
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

