using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Domain.Enums;

namespace Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<List<OrderResponse>> GetAllAsync();
        Task<OrderResponse?> GetByIdAsync(int id);
        Task<OrderResponse?> GetByOrderNumberAsync(string orderNumber);
        Task<List<OrderResponse>> GetByStatusAsync(OrderStatus status);
        Task<List<OrderResponse>> GetByUserIdAsync(int userId);
        Task<int> CreateAsync(CreateOrderRequest request);
        Task UpdateStatusAsync(int id, UpdateOrderStatusRequest request);
        Task ConfirmAsync(int id);
        Task CancelAsync(int id, string reason);
        Task CompleteAsync(int id);
        Task DeleteAsync(int id);
    }
}
