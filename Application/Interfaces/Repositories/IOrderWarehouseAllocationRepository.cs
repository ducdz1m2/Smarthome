using Domain.Entities.Sales;

namespace Application.Interfaces.Repositories
{
    public interface IOrderWarehouseAllocationRepository
    {
        Task<OrderWarehouseAllocation?> GetByIdAsync(int id);
        Task<List<OrderWarehouseAllocation>> GetByOrderItemIdAsync(int orderItemId);
        Task<List<OrderWarehouseAllocation>> GetByOrderIdAsync(int orderId);
        Task AddAsync(OrderWarehouseAllocation allocation);
        void Update(OrderWarehouseAllocation allocation);
        void Delete(OrderWarehouseAllocation allocation);
        Task SaveChangesAsync();
    }
}
