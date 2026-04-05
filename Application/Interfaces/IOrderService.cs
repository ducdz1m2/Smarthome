using Application.DTOs;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllAsync();
        Task<OrderDto?> GetByIdAsync(int id);
        Task<List<OrderDto>> GetByUserAsync(int userId);
        Task<int> CreateAsync(CreateOrderRequest request);
        Task UpdateStatusAsync(int id, UpdateOrderStatusRequest request);
        Task CancelAsync(int id, string reason);
    }
}
