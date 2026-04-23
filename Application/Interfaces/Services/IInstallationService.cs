using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IInstallationService
    {
        Task<List<InstallationBookingListResponse>> GetAllAsync();
        Task<InstallationBookingResponse?> GetByIdAsync(int id);
        Task<InstallationBookingResponse?> GetByOrderIdAsync(int orderId);
        Task<List<InstallationBookingResponse>> GetListByOrderIdAsync(int orderId);
        Task<(List<InstallationBookingListResponse> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, int? technicianId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<InstallationBookingListResponse>> GetByTechnicianAsync(int technicianId);
        Task<List<InstallationBookingListResponse>> GetByStatusAsync(string status);
        Task<int> CreateAsync(CreateInstallationBookingRequest request);
        Task UpdateAsync(int id, UpdateInstallationBookingRequest request);
        Task StartWarrantyAsync(int id);
        Task AssignTechnicianAsync(int id, int technicianId, int slotId);
        Task StartPreparationAsync(int id);
        Task StartTravelAsync(int id);
        Task StartInstallationAsync(int id);
        Task CompleteAsync(int id, CompleteInstallationRequest request);
        Task RescheduleAsync(int id, RescheduleInstallationRequest request);
        Task CustomerRescheduleAsync(int id, RescheduleInstallationRequest request);
        Task AcceptRescheduledAsync(int id);
        Task CancelAsync(int id, CancelInstallationRequest request);
        Task AddMaterialAsync(int bookingId, AddInstallationMaterialRequest request);
        Task RecordMaterialUsageAsync(int bookingId, RecordMaterialUsageRequest request);
        Task DeleteAsync(int id);

        // Technician acceptance flow
        Task AcceptBookingAsync(int bookingId, int technicianId);
        Task RejectBookingAsync(int bookingId, int technicianId, RejectBookingRequest request);
        Task ReportOutOfStockAsync(int bookingId, int technicianId);
        Task ResetFromAwaitingMaterialAsync(int bookingId, DateTime? newScheduledDate = null);
        Task FailBookingAsync(int bookingId, string reason);
        Task<List<InstallationBookingListResponse>> GetPendingForTechnicianAsync(int technicianId);

        // Warehouse material preparation flow
        Task PrepareMaterialsFromWarehouseAsync(int bookingId, PrepareMaterialsRequest request);
        Task ReturnMaterialsToWarehouseAsync(int bookingId, List<MaterialReturnInfo> returns);

        // Uninstall booking management
        Task SetIsUninstallAsync(int bookingId, bool isUninstall);
        Task UpdateIsWarrantyAsync(int bookingId, bool isWarranty);
    }
}
