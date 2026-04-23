using Domain.Entities.Communication;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class PushSubscriptionRepository : IPushSubscriptionRepository
{
    private readonly AppDbContext _context;

    public PushSubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PushSubscription?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.PushSubscriptions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<List<PushSubscription>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.PushSubscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PushSubscription?> GetByEndpointAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        return await _context.PushSubscriptions
            .FirstOrDefaultAsync(s => s.Endpoint == endpoint, cancellationToken);
    }

    public async Task AddAsync(PushSubscription subscription, CancellationToken cancellationToken = default)
    {
        await _context.PushSubscriptions.AddAsync(subscription, cancellationToken);
    }

    public void Update(PushSubscription subscription)
    {
        _context.PushSubscriptions.Update(subscription);
    }

    public void Delete(PushSubscription subscription)
    {
        _context.PushSubscriptions.Remove(subscription);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
