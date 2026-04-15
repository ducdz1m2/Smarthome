using Domain.Entities.Installation;
using Domain.Enums;

namespace Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for InstallationBooking - temporary copy from Application layer
    /// </summary>
    public interface IInstallationBookingRepository
    {
        Task<InstallationBooking?> GetByIdAsync(int id);
        Task<InstallationBooking?> GetByIdWithDetailsAsync(int id);
        Task<List<InstallationBooking>> GetAllAsync();
        Task<List<InstallationBooking>> GetByTechnicianIdAsync(int technicianId);
        Task<List<InstallationBooking>> GetByStatusAsync(InstallationStatus status);
        Task<List<InstallationBooking>> GetByOrderIdAsync(int orderId);
        Task<List<InstallationBooking>> GetPendingAsync();
        Task<List<InstallationBooking>> GetOverdueAsync();
        Task<List<InstallationBooking>> GetScheduledForDateAsync(DateTime date);
        Task AddAsync(InstallationBooking booking);
        void Update(InstallationBooking booking);
        void Delete(InstallationBooking booking);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
        Task<int> CountByStatusAsync(InstallationStatus status);
        Task SaveChangesAsync();
    }
}
