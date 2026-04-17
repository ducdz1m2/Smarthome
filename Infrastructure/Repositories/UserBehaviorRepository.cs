using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserBehaviorRepository : Application.Interfaces.Repositories.IUserBehaviorRepository
{
    private readonly AppDbContext _context;

    public UserBehaviorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserBehavior?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.UserBehaviors.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<UserBehavior>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserBehaviors.ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.UserBehaviors.AnyAsync(b => b.Id == id, cancellationToken);
    }

    public void Add(UserBehavior entity)
    {
        _context.UserBehaviors.Add(entity);
    }

    public void Update(UserBehavior entity)
    {
        _context.UserBehaviors.Update(entity);
    }

    public void Delete(UserBehavior entity)
    {
        _context.UserBehaviors.Remove(entity);
    }

    public IQueryable<UserBehavior> Query(CancellationToken cancellationToken = default)
    {
        return _context.UserBehaviors.AsQueryable();
    }

    public async Task<IReadOnlyList<UserBehavior>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserBehaviors
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserBehavior>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _context.UserBehaviors
            .Where(b => b.ProductId == productId)
            .OrderByDescending(b => b.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserBehavior>> GetByUserIdAndProductIdAsync(int userId, int productId, CancellationToken cancellationToken = default)
    {
        return await _context.UserBehaviors
            .Where(b => b.UserId == userId && b.ProductId == productId)
            .OrderByDescending(b => b.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserBehavior>> GetByBehaviorTypeAsync(BehaviorType behaviorType, CancellationToken cancellationToken = default)
    {
        return await _context.UserBehaviors
            .Where(b => b.BehaviorType == behaviorType)
            .OrderByDescending(b => b.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserBehavior>> GetForTrainingAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.UserBehaviors.AsQueryable();
        
        if (fromDate.HasValue)
        {
            query = query.Where(b => b.Timestamp >= fromDate.Value);
        }

        return await query
            .OrderBy(b => b.UserId)
            .ThenBy(b => b.ProductId)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserBehavior?> GetLatestByUserAndProductAsync(int userId, int productId, CancellationToken cancellationToken = default)
    {
        return await _context.UserBehaviors
            .Where(b => b.UserId == userId && b.ProductId == productId)
            .OrderByDescending(b => b.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
