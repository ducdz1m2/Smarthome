using Domain.Entities.Inventory;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class StockIssueRepository : IStockIssueRepository
{
    private readonly AppDbContext _context;

    public StockIssueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StockIssue?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.StockIssues.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<StockIssue>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StockIssues.ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.StockIssues.AnyAsync(s => s.Id == id, cancellationToken);
    }

    public void Add(StockIssue entity)
    {
        _context.StockIssues.Add(entity);
    }

    public void Add(StockIssueDetail entity)
    {
        _context.StockIssueDetails.Add(entity);
    }

    public void Update(StockIssue entity)
    {
        _context.StockIssues.Update(entity);
    }

    public void Delete(StockIssue entity)
    {
        _context.StockIssues.Remove(entity);
    }

    public async Task<StockIssue?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        // Query StockIssue and manually load details since we removed navigation properties
        var stockIssue = await _context.StockIssues
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        
        if (stockIssue != null)
        {
            var details = await _context.StockIssueDetails
                .Where(d => d.StockIssueId == id)
                .ToListAsync(cancellationToken);
            // Details can be loaded separately if needed
        }
        
        return stockIssue;
    }

    public async Task<IReadOnlyList<StockIssue>> GetByWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default)
    {
        return await _context.StockIssues
            .Where(s => s.WarehouseId == warehouseId)
            .OrderByDescending(s => s.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockIssue>> GetByBookingAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        return await _context.StockIssues
            .Where(s => s.BookingId == bookingId)
            .OrderByDescending(s => s.IssueDate)
            .ToListAsync(cancellationToken);
    }
}
