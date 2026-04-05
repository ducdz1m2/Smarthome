using Application.DTOs;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<List<CategoryDto>> GetTreeAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<List<CategoryDto>> GetChildrenAsync(int parentId);
        Task<int> CreateAsync(CreateCategoryRequest request);
        Task UpdateAsync(int id, UpdateCategoryRequest request);
        Task DeleteAsync(int id);
    }
}
