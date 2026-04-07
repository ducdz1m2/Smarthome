using Domain.Entities.Installation;

namespace Application.Interfaces.Repositories
{
    public interface IInstallationBookingRepository
    {
        Task<InstallationBooking?> GetByIdAsync(int id);
        Task<InstallationBooking?> GetByIdWithDetailsAsync(int id);
        Task<InstallationBooking?> GetByOrderIdAsync(int orderId);
        Task<List<InstallationBooking>> GetAllAsync();
        Task<List<InstallationBooking>> GetByTechnicianIdAsync(int technicianId);
        Task<List<InstallationBooking>> GetByStatusAsync(string status);
        Task<(List<InstallationBooking> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, int? technicianId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByOrderIdAsync(int orderId);
        Task AddAsync(InstallationBooking booking);
        void Update(InstallationBooking booking);
        void Delete(InstallationBooking booking);
        Task<int> CountAsync();
        Task<int> CountByStatusAsync(string status);
        Task SaveChangesAsync();
    }
}
