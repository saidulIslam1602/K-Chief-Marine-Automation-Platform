using System.Linq.Expressions;

namespace KChief.DataAccess.Interfaces;

/// <summary>
/// Generic repository interface for common CRUD operations.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Gets entities with optional filtering, ordering, and including related data.
    /// </summary>
    Task<IEnumerable<T>> GetAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string includeProperties = "");

    /// <summary>
    /// Gets an entity by ID.
    /// </summary>
    Task<T?> GetByIdAsync(object id);

    /// <summary>
    /// Gets the first entity matching the filter.
    /// </summary>
    Task<T?> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>>? filter = null,
        string includeProperties = "");

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Adds multiple entities.
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Updates an entity.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Updates multiple entities.
    /// </summary>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    Task DeleteAsync(object id);

    /// <summary>
    /// Deletes multiple entities.
    /// </summary>
    void DeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// Checks if any entity matches the filter.
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);

    /// <summary>
    /// Gets the count of entities matching the filter.
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);
}
