using Domain.Entities.Sales;
using Domain.Enums;

namespace Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Order - temporary copy from Application layer
    /// </summary>
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<Order?> GetByIdWithDetailsAsync(int id);
        Task<Order?> GetByIdWithDetailsForUpdateAsync(int id);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<List<Order>> GetAllAsync();
        Task<List<Order>> GetByStatusAsync(OrderStatus status);
        Task<List<Order>> GetByUserIdAsync(int userId);
        Task AddAsync(Order order);
        void Update(Order order);
        void Delete(Order order);
        Task<bool> ExistsAsync(string orderNumber);
        Task<int> CountAsync();
        Task<int> CountByStatusAsync(OrderStatus status);
        Task SaveChangesAsync();
    }
}
