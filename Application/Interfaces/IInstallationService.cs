using Application.DTOs;

namespace Application.Interfaces
{
    public interface IInstallationService
    {
        Task<List<InstallationBookingDto>> GetAllAsync();
        Task<InstallationBookingDto?> GetByIdAsync(int id);
        Task<List<InstallationBookingDto>> GetByOrderAsync(int orderId);
        Task<List<InstallationBookingDto>> GetByTechnicianAsync(int technicianId);
        Task<int> BookAsync(BookInstallationRequest request);
        Task AssignTechnicianAsync(int bookingId, int technicianId);
        Task StartJobAsync(int bookingId);
        Task CompleteJobAsync(int bookingId, string customerSignature);
        Task CancelAsync(int bookingId, string reason);
        
        Task<List<TechnicianDto>> GetTechniciansAsync();
        Task<List<InstallationSlotDto>> GetAvailableSlotsAsync(DateTime date, string district);
    }
}
