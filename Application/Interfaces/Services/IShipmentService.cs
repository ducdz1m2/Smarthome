using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services;

public interface IShipmentService
{
    // Query methods
    Task<List<OrderShipmentResponse>> GetAllAsync();
    Task<OrderShipmentResponse?> GetByIdAsync(int id);
    Task<OrderShipmentResponse?> GetByOrderIdAsync(int orderId);
    Task<OrderShipmentResponse?> GetByTrackingNumberAsync(string trackingNumber);
    Task<List<OrderShipmentResponse>> GetByStatusAsync(string status);
    Task<List<OrderShipmentResponse>> GetByCarrierAsync(string carrier);
    
    // Command methods
    Task<int> CreateAsync(CreateShipmentRequest request);
    Task UpdateTrackingAsync(int id, UpdateTrackingRequest request);
    Task MarkPickedUpAsync(int id);
    Task MarkDeliveredAsync(int id);
    Task DeleteAsync(int id);
    
    // Admin approval methods
    Task ApproveShipmentAsync(int shipmentId, int approvedBy);
    Task RejectShipmentAsync(int shipmentId, string reason);
    
    // Shipper assignment methods
    Task AssignShipperAsync(int shipmentId, int shipperId);
    Task AutoAssignShipmentAsync(int shipmentId, int shipperId);
}
