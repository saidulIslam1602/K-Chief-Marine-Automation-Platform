using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using HMI.DataAccess.Data;
using HMI.DataAccess.Interfaces;

namespace HMI.DataAccess.Repositories;

/// <summary>
/// Generic repository implementation for common CRUD operations.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string includeProperties = "")
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split(
            new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            return await orderBy(query).ToListAsync();
        }
        else
        {
            return await query.ToListAsync();
        }
    }

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T?> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>>? filter = null,
        string includeProperties = "")
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split(
            new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        return await query.FirstOrDefaultAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var result = await _dbSet.AddAsync(entity);
        return result.Entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            Update(entity);
        }
    }

    public virtual void Delete(T entity)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        _dbSet.Remove(entity);
    }

    public virtual async Task DeleteAsync(object id)
    {
        T? entityToDelete = await _dbSet.FindAsync(id);
        if (entityToDelete != null)
        {
            Delete(entityToDelete);
        }
    }

    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
    {
        return await _dbSet.AnyAsync(filter);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
    {
        if (filter != null)
        {
            return await _dbSet.CountAsync(filter);
        }
        return await _dbSet.CountAsync();
    }
}
