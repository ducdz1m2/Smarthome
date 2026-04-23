using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.DTOs.Responses.Catalog;

namespace Application.Interfaces.Services
{
    public interface ICategoryService
    {
        //Queries
        Task<List<CategoryResponse>> GetAllAsync();
        Task<List<CategoryResponse>> GetCategoriesWithProductsAsync();
        Task<CategoryResponse?> GetByIdAsync(int id);
        Task<List<CategoryTreeResponse>> GetTreeAsync();
        
        //Commands
        Task<int> CreateAsync(CreateCategoryRequest request);
        Task UpdateAsync(int id, UpdateCategoryRequest request);
        Task DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
