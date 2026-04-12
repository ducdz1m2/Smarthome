namespace Domain.Repositories;

/// <summary>
/// Unit of Work pattern interface for transaction management.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Extended Unit of Work with domain event dispatching support.
/// </summary>
public interface IUnitOfWorkWithEvents : IUnitOfWork
{
    Task SaveChangesAndDispatchEventsAsync(CancellationToken cancellationToken = default);
}
