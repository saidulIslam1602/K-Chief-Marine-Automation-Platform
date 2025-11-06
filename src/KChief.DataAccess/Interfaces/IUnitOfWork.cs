namespace KChief.DataAccess.Interfaces;

/// <summary>
/// Unit of Work interface for managing transactions and repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Vessel repository.
    /// </summary>
    IVesselRepository Vessels { get; }

    /// <summary>
    /// Engine repository.
    /// </summary>
    IEngineRepository Engines { get; }

    /// <summary>
    /// Sensor repository.
    /// </summary>
    ISensorRepository Sensors { get; }

    /// <summary>
    /// Alarm repository.
    /// </summary>
    IAlarmRepository Alarms { get; }

    /// <summary>
    /// Message bus event repository.
    /// </summary>
    IMessageBusEventRepository MessageBusEvents { get; }

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Begins a database transaction.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync();
}
