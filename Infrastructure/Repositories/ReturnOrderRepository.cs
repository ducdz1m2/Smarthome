using Application.Interfaces.Repositories;
using Domain.Entities.Sales;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReturnOrderRepository : IReturnOrderRepository
{
    private readonly AppDbContext _context;

    public ReturnOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReturnOrder?> GetByIdAsync(int id)
        => await _context.ReturnOrders.FindAsync(id);

    public async Task<ReturnOrder?> GetByIdWithItemsAsync(int id)
        => await _context.ReturnOrders
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<ReturnOrder>> GetAllAsync()
        => await _context.ReturnOrders.ToListAsync();

    public async Task<List<ReturnOrder>> GetByOrderIdAsync(int orderId)
        => await _context.ReturnOrders
            .Where(r => r.OriginalOrderId == orderId)
            .ToListAsync();

    public async Task<List<ReturnOrder>> GetByStatusAsync(ReturnOrderStatus status)
        => await _context.ReturnOrders
            .Where(r => r.Status == status)
            .ToListAsync();

    public async Task<bool> ExistsPendingReturnForOrderAsync(int orderId)
        => await _context.ReturnOrders
            .AnyAsync(r => r.OriginalOrderId == orderId && r.Status == ReturnOrderStatus.Pending);

    public async Task AddAsync(ReturnOrder returnOrder)
        => await _context.ReturnOrders.AddAsync(returnOrder);

    public void Update(ReturnOrder returnOrder)
        => _context.ReturnOrders.Update(returnOrder);

    public void Delete(ReturnOrder returnOrder)
        => _context.ReturnOrders.Remove(returnOrder);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
