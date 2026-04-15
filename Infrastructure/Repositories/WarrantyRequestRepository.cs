using Application.Interfaces.Repositories;
using Domain.Entities.Sales;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class WarrantyRequestRepository : IWarrantyRequestRepository
{
    private readonly AppDbContext _context;

    public WarrantyRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WarrantyRequest?> GetByIdAsync(int id)
        => await _context.WarrantyRequests.FindAsync(id);

    public async Task<WarrantyRequest?> GetByIdWithItemsAsync(int id)
        => await _context.WarrantyRequests
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<WarrantyRequest>> GetAllAsync()
        => await _context.WarrantyRequests.ToListAsync();

    public async Task<List<WarrantyRequest>> GetByOrderIdAsync(int orderId)
        => await _context.WarrantyRequests
            .Where(r => r.OrderId == orderId)
            .ToListAsync();

    public async Task<List<WarrantyRequest>> GetByStatusAsync(WarrantyRequestStatus status)
        => await _context.WarrantyRequests
            .Where(r => r.Status == status)
            .ToListAsync();

    public async Task<bool> ExistsPendingWarrantyForOrderAsync(int orderId)
        => await _context.WarrantyRequests
            .AnyAsync(r => r.OrderId == orderId && r.Status == WarrantyRequestStatus.Pending);

    public async Task AddAsync(WarrantyRequest warrantyRequest)
        => await _context.WarrantyRequests.AddAsync(warrantyRequest);

    public void Update(WarrantyRequest warrantyRequest)
        => _context.WarrantyRequests.Update(warrantyRequest);

    public void Delete(WarrantyRequest warrantyRequest)
        => _context.WarrantyRequests.Remove(warrantyRequest);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
