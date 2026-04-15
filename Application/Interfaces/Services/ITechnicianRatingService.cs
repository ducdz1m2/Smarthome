using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface ITechnicianRatingService
    {
        Task<List<TechnicianRatingResponse>> GetAllAsync();
        Task<List<TechnicianRatingResponse>> GetByTechnicianAsync(int technicianId);
        Task<List<TechnicianRatingResponse>> GetByUserAsync(int userId);
        Task<List<TechnicianRatingResponse>> GetByBookingAsync(int bookingId);
        Task<TechnicianRatingResponse?> GetByTechnicianAndBookingAsync(int technicianId, int bookingId);
        Task<List<TechnicianRatingResponse>> GetPendingApprovalAsync();
        Task<TechnicianRatingResponse?> GetByIdAsync(int id);
        Task<int> CountAsync();
        Task<int> CountPendingAsync();
        Task<TechnicianRatingResponse> CreateAsync(CreateTechnicianRatingRequest request);
        Task<TechnicianRatingResponse> UpdateAsync(int id, CreateTechnicianRatingRequest request);
        Task ApproveAsync(int id);
        Task RejectAsync(int id);
        Task DeleteAsync(int id);
        Task DeleteByIdAsync(int id);
    }
}
