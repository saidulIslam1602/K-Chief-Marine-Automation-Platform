using Microsoft.EntityFrameworkCore.Storage;
using HMI.DataAccess.Data;
using HMI.DataAccess.Interfaces;
using HMI.DataAccess.Repositories;

namespace HMI.DataAccess.Services;

/// <summary>
/// Unit of Work implementation for managing transactions and repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    // Repository instances
    private IVesselRepository? _vessels;
    private IEngineRepository? _engines;
    private ISensorRepository? _sensors;
    private IAlarmRepository? _alarms;
    private IMessageBusEventRepository? _messageBusEvents;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IVesselRepository Vessels
    {
        get
        {
            _vessels ??= new VesselRepository(_context);
            return _vessels;
        }
    }

    public IEngineRepository Engines
    {
        get
        {
            _engines ??= new EngineRepository(_context);
            return _engines;
        }
    }

    public ISensorRepository Sensors
    {
        get
        {
            _sensors ??= new SensorRepository(_context);
            return _sensors;
        }
    }

    public IAlarmRepository Alarms
    {
        get
        {
            _alarms ??= new AlarmRepository(_context);
            return _alarms;
        }
    }

    public IMessageBusEventRepository MessageBusEvents
    {
        get
        {
            _messageBusEvents ??= new MessageBusEventRepository(_context);
            return _messageBusEvents;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
