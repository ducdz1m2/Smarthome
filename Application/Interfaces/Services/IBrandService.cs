using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IBrandService
    {
        Task<List<BrandResponse>> GetAllAsync();
        Task<BrandResponse?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateBrandRequest request);
        Task UpdateAsync(int id, UpdateBrandRequest request);
        Task DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
