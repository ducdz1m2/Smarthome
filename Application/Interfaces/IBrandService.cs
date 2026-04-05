using Application.DTOs;

namespace Application.Interfaces
{
    public interface IBrandService
    {
        Task<List<BrandDto>> GetAllAsync();
        Task<BrandDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateBrandRequest request);
        Task UpdateAsync(int id, UpdateBrandRequest request);
        Task DeleteAsync(int id);
    }
}
