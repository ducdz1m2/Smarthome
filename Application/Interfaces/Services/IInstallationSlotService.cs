using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IInstallationSlotService
    {
        Task<List<InstallationSlotListResponse>> GetAllAsync();
        Task<InstallationSlotResponse?> GetByIdAsync(int id);
        Task<List<InstallationSlotResponse>> GetByTechnicianAsync(int technicianId);
        Task<List<InstallationSlotResponse>> GetByTechnicianAndDateAsync(int technicianId, DateTime date);
        Task<List<InstallationSlotResponse>> GetAvailableSlotsAsync(int technicianId, DateTime date);
        Task<(List<InstallationSlotListResponse> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, int? technicianId = null, DateTime? date = null, bool? isBooked = null);
        Task<int> CreateAsync(CreateInstallationSlotRequest request);
        Task CreateBatchAsync(BatchCreateSlotRequest request);
        Task UpdateAsync(int id, UpdateInstallationSlotRequest request);
        Task ReleaseAsync(int id);
        Task DeleteAsync(int id);
        Task DeleteByTechnicianAndDateAsync(int technicianId, DateTime date);
    }
}
