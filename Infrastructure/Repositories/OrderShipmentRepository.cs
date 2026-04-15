using Application.Interfaces.Repositories;
using Domain.Entities.Sales;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderShipmentRepository : IOrderShipmentRepository
{
    private readonly AppDbContext _context;

    public OrderShipmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderShipment?> GetByIdAsync(int id)
        => await _context.OrderShipments.FindAsync(id);

    public async Task<OrderShipment?> GetByOrderIdAsync(int orderId)
        => await _context.OrderShipments
            .FirstOrDefaultAsync(s => s.OrderId == orderId);

    public async Task<OrderShipment?> GetByTrackingNumberAsync(string trackingNumber)
        => await _context.OrderShipments
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);

    public async Task<List<OrderShipment>> GetAllAsync()
        => await _context.OrderShipments.ToListAsync();

    public async Task<List<OrderShipment>> GetByStatusAsync(OrderShipmentStatus status)
        => await _context.OrderShipments
            .Where(s => s.Status == status)
            .ToListAsync();

    public async Task<List<OrderShipment>> GetByCarrierAsync(string carrier)
        => await _context.OrderShipments
            .Where(s => s.Carrier == carrier)
            .ToListAsync();

    public async Task AddAsync(OrderShipment shipment)
        => await _context.OrderShipments.AddAsync(shipment);

    public void Update(OrderShipment shipment)
        => _context.OrderShipments.Update(shipment);

    public void Delete(OrderShipment shipment)
        => _context.OrderShipments.Remove(shipment);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
