using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Domain.Enums;

namespace Application.Interfaces.Services;

public interface IReturnOrderService
{
    // Query methods
    Task<List<ReturnOrderResponse>> GetAllAsync();
    Task<ReturnOrderResponse?> GetByIdAsync(int id);
    Task<List<ReturnOrderResponse>> GetByOrderIdAsync(int orderId);
    Task<List<ReturnOrderResponse>> GetByStatusAsync(string status);
    
    // Command methods
    Task<int> CreateAsync(CreateReturnOrderRequest request);
    Task ApproveAsync(int id, decimal? refundAmount = null);
    Task RejectAsync(int id, string? reason = null);
    Task MarkReceivedAsync(int id);
    Task CompleteAsync(int id);
    Task CancelAsync(int id, string? reason = null);
    Task DeleteAsync(int id);
}
