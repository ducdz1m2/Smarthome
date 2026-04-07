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
        Task ProcessAsync(int id);
        Task ShipAsync(int id);
        Task DeliverAsync(int id);
        Task ScheduleInstallationAsync(int id);
        Task AssignTechnicianAsync(int id);
        Task PrepareAsync(int id);
        Task StartInstallationAsync(int id);
        Task StartTestingAsync(int id);
        Task CompleteAsync(int id);
        Task CancelAsync(int id, string reason);
        Task ReturnAsync(int id, string reason);
        Task RefundAsync(int id, string reason);
        Task DeleteAsync(int id);
    }
}
