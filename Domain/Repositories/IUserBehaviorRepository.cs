using Domain.Entities;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for UserBehavior entity
/// </summary>
public interface IUserBehaviorRepository : IQueryableRepository<UserBehavior>
{
    Task<IReadOnlyList<UserBehavior>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserBehavior>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserBehavior>> GetByUserIdAndProductIdAsync(int userId, int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserBehavior>> GetByBehaviorTypeAsync(Domain.Enums.BehaviorType behaviorType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserBehavior>> GetForTrainingAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default);
    Task<UserBehavior?> GetLatestByUserAndProductAsync(int userId, int productId, CancellationToken cancellationToken = default);
}
