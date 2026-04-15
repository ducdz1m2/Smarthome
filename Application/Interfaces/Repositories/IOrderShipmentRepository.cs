using Domain.Entities.Sales;

namespace Application.Interfaces.Repositories;

public interface IOrderShipmentRepository
{
    Task<OrderShipment?> GetByIdAsync(int id);
    Task<OrderShipment?> GetByOrderIdAsync(int orderId);
    Task<OrderShipment?> GetByTrackingNumberAsync(string trackingNumber);
    Task<List<OrderShipment>> GetAllAsync();
    Task<List<OrderShipment>> GetByStatusAsync(OrderShipmentStatus status);
    Task<List<OrderShipment>> GetByCarrierAsync(string carrier);
    Task AddAsync(OrderShipment shipment);
    void Update(OrderShipment shipment);
    void Delete(OrderShipment shipment);
    Task SaveChangesAsync();
}
