using Domain.Abstractions;

namespace Domain.Repositories;

/// <summary>
/// Base repository interface for all entities.
/// </summary>
public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

/// <summary>
/// Extended repository interface with query capabilities.
/// </summary>
public interface IQueryableRepository<T> : IRepository<T> where T : Entity
{
    IQueryable<T> Query(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for entities supporting soft delete.
/// </summary>
public interface ISoftDeleteRepository<T> : IRepository<T> where T : Entity, ISoftDeletable
{
    Task<T?> GetByIdIncludingDeletedAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
}
