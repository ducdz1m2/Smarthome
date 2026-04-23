using Domain.Entities.Communication;

namespace Domain.Repositories;

public interface IPushSubscriptionRepository
{
    Task<PushSubscription?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<PushSubscription>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<PushSubscription?> GetByEndpointAsync(string endpoint, CancellationToken cancellationToken = default);
    Task AddAsync(PushSubscription subscription, CancellationToken cancellationToken = default);
    void Update(PushSubscription subscription);
    void Delete(PushSubscription subscription);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
