using Domain.Entities.Installation;

namespace Application.Interfaces.Repositories
{
    public interface IInstallationSlotRepository
    {
        Task<InstallationSlot?> GetByIdAsync(int id);
        Task<List<InstallationSlot>> GetAllAsync();
        Task<List<InstallationSlot>> GetByTechnicianIdAsync(int technicianId);
        Task<List<InstallationSlot>> GetByTechnicianAndDateAsync(int technicianId, DateTime date);
        Task<List<InstallationSlot>> GetAvailableSlotsAsync(int technicianId, DateTime date);
        Task<(List<InstallationSlot> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, int? technicianId = null, DateTime? date = null, bool? isBooked = null);
        Task<bool> ExistsAsync(int id);
        Task<bool> HasOverlapAsync(int technicianId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
        Task AddAsync(InstallationSlot slot);
        Task AddRangeAsync(List<InstallationSlot> slots);
        void Update(InstallationSlot slot);
        void Delete(InstallationSlot slot);
        void DeleteRange(List<InstallationSlot> slots);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
