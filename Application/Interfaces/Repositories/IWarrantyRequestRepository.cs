using Domain.Entities.Sales;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface IWarrantyRequestRepository
{
    Task<WarrantyRequest?> GetByIdAsync(int id);
    Task<WarrantyRequest?> GetByIdWithItemsAsync(int id);
    Task<List<WarrantyRequest>> GetAllAsync();
    Task<List<WarrantyRequest>> GetByOrderIdAsync(int orderId);
    Task<WarrantyRequest?> GetByBookingIdAsync(int bookingId);
    Task<List<WarrantyRequest>> GetByStatusAsync(WarrantyRequestStatus status);
    Task<bool> ExistsPendingWarrantyForOrderAsync(int orderId);
    Task AddAsync(WarrantyRequest warrantyRequest);
    void Update(WarrantyRequest warrantyRequest);
    void Delete(WarrantyRequest warrantyRequest);
    Task SaveChangesAsync();
}
