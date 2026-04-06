using Domain.Entities.Sales;
using Domain.Enums;

namespace Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<Order?> GetByIdWithDetailsAsync(int id);
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
