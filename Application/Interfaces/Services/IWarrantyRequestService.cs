using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Domain.Enums;

namespace Application.Interfaces.Services;

public interface IWarrantyRequestService
{
    Task<List<WarrantyRequestResponse>> GetAllAsync();
    Task<WarrantyRequestResponse?> GetByIdAsync(int id);
    Task<List<WarrantyRequestResponse>> GetByOrderIdAsync(int orderId);
    Task<List<WarrantyRequestResponse>> GetByStatusAsync(string status);
    Task<int> CreateAsync(CreateWarrantyRequestRequest request);
    Task ApproveAsync(int id);
    Task RejectAsync(int id, string reason);
    Task StartAsync(int id);
    Task CompleteAsync(int id, string? technicianNotes = null);
    Task MarkItemAsReturnedAsync(int itemId, int? warehouseId = null);
    Task UpdateDamagedItemStatusAsync(int itemId, DamagedProductStatus status, int? warehouseId = null, decimal? repairCost = null, string? repairNotes = null);
}
