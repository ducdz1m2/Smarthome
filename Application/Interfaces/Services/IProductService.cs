using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IProductService
    {
        //Queries
        Task<List<ProductResponse>> GetAllAsync();
        Task<ProductResponse?> GetByIdAsync(int id);
        Task<List<ProductResponse>> GetByCategoryAsync(int categoryId);
        Task<List<ProductResponse>> SearchAsync(string keyword, string? filters);

        //Commands
        Task<int> CreateAsync(CreateProductRequest request);
        Task UpdateAsync(int id, UpdateProductRequest request);
        Task DeleteAsync(int id);
        Task<bool> UpdateStockAsync(int productId, int quantity);

    }
}
