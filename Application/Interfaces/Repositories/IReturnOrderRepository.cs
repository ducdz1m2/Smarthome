using Domain.Entities.Sales;

namespace Application.Interfaces.Repositories;

public interface IReturnOrderRepository
{
    Task<ReturnOrder?> GetByIdAsync(int id);
    Task<ReturnOrder?> GetByIdWithItemsAsync(int id);
    Task<List<ReturnOrder>> GetAllAsync();
    Task<List<ReturnOrder>> GetByOrderIdAsync(int orderId);
    Task<List<ReturnOrder>> GetByStatusAsync(ReturnOrderStatus status);
    Task<bool> ExistsPendingReturnForOrderAsync(int orderId);
    Task AddAsync(ReturnOrder returnOrder);
    void Update(ReturnOrder returnOrder);
    void Delete(ReturnOrder returnOrder);
    Task SaveChangesAsync();
}
