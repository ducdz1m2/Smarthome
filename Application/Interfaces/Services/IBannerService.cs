using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IBannerService
    {
        Task<List<BannerResponse>> GetAllAsync();
        Task<BannerResponse?> GetByIdAsync(int id);
        Task<List<BannerResponse>> GetByPositionAsync(string position);
        Task<int> CreateAsync(CreateBannerRequest request);
        Task UpdateAsync(int id, UpdateBannerRequest request);
        Task DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
