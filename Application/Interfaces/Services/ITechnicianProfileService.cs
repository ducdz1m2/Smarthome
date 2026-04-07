using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface ITechnicianProfileService
    {
        Task<List<TechnicianListResponse>> GetAllAsync();
        Task<TechnicianResponse?> GetByIdAsync(int id);
        Task<TechnicianResponse?> GetByUserIdAsync(int userId);
        Task<(List<TechnicianListResponse> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, bool? isAvailable = null, string? search = null);
        Task<List<TechnicianResponse>> GetAvailableAsync();
        Task<List<TechnicianResponse>> GetByDistrictAsync(string district);
        Task<int> CreateAsync(CreateTechnicianProfileRequest request);
        Task UpdateAsync(int id, UpdateTechnicianProfileRequest request);
        Task AddSkillAsync(int id, AddTechnicianSkillRequest request);
        Task SetAvailableAsync(int id, bool available);
        Task DeleteAsync(int id);
    }
}
