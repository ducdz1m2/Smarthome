using Application.Interfaces.Repositories;
using Domain.Entities.Sales;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderWarehouseAllocationRepository : IOrderWarehouseAllocationRepository
{
    private readonly AppDbContext _context;

    public OrderWarehouseAllocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderWarehouseAllocation?> GetByIdAsync(int id)
    {
        return await _context.OrderWarehouseAllocations.FindAsync(id);
    }

    public async Task<List<OrderWarehouseAllocation>> GetByOrderItemIdAsync(int orderItemId)
    {
        return await _context.OrderWarehouseAllocations
            .Where(owa => owa.OrderItemId == orderItemId)
            .ToListAsync();
    }

    public async Task<List<OrderWarehouseAllocation>> GetByOrderIdAsync(int orderId)
    {
        return await _context.OrderWarehouseAllocations
            .Include(owa => owa.OrderItem)
            .Where(owa => owa.OrderItem.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<List<OrderWarehouseAllocation>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await _context.OrderWarehouseAllocations
            .Where(owa => owa.WarehouseId == warehouseId)
            .ToListAsync();
    }

    public async Task AddAsync(OrderWarehouseAllocation allocation)
    {
        await _context.OrderWarehouseAllocations.AddAsync(allocation);
    }

    public void Update(OrderWarehouseAllocation allocation)
    {
        _context.OrderWarehouseAllocations.Update(allocation);
    }

    public void Delete(OrderWarehouseAllocation allocation)
    {
        _context.OrderWarehouseAllocations.Remove(allocation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
