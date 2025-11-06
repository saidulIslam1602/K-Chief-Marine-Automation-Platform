namespace KChief.Platform.Core.Interfaces;

/// <summary>
/// Interface for Modbus client operations.
/// </summary>
public interface IModbusClient
{
    /// <summary>
    /// Connects to a Modbus TCP server.
    /// </summary>
    Task<bool> ConnectAsync(string ipAddress, int port = 502);

    /// <summary>
    /// Disconnects from the Modbus server.
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Checks if the client is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Reads holding registers from the Modbus device.
    /// </summary>
    Task<ushort[]?> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort numberOfRegisters);

    /// <summary>
    /// Reads input registers from the Modbus device.
    /// </summary>
    Task<ushort[]?> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort numberOfRegisters);

    /// <summary>
    /// Reads discrete inputs from the Modbus device.
    /// </summary>
    Task<bool[]?> ReadDiscreteInputsAsync(byte slaveId, ushort startAddress, ushort numberOfInputs);

    /// <summary>
    /// Reads coils from the Modbus device.
    /// </summary>
    Task<bool[]?> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort numberOfCoils);

    /// <summary>
    /// Writes a single holding register.
    /// </summary>
    Task<bool> WriteSingleRegisterAsync(byte slaveId, ushort registerAddress, ushort value);

    /// <summary>
    /// Writes multiple holding registers.
    /// </summary>
    Task<bool> WriteMultipleRegistersAsync(byte slaveId, ushort startAddress, ushort[] values);

    /// <summary>
    /// Writes a single coil.
    /// </summary>
    Task<bool> WriteSingleCoilAsync(byte slaveId, ushort coilAddress, bool value);
}

