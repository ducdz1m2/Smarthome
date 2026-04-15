using Domain.Entities.Installation;

namespace Application.Interfaces.Repositories
{
    public interface ITechnicianRatingRepository
    {
        Task<TechnicianRating?> GetByIdAsync(int id);
        Task<List<TechnicianRating>> GetAllAsync();
        Task<List<TechnicianRating>> GetByTechnicianAsync(int technicianId);
        Task<List<TechnicianRating>> GetByUserAsync(int userId);
        Task<List<TechnicianRating>> GetByBookingAsync(int bookingId);
        Task<TechnicianRating?> GetByTechnicianAndBookingAsync(int technicianId, int bookingId);
        Task<List<TechnicianRating>> GetPendingApprovalAsync();
        Task<int> CountAsync();
        Task<int> CountPendingAsync();
        Task AddAsync(TechnicianRating rating);
        void Update(TechnicianRating rating);
        void Delete(TechnicianRating rating);
        Task DeleteByIdAsync(int id);
        Task SaveChangesAsync();
    }
}
