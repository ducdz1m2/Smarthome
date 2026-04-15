using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IProductVariantService
    {
        Task<List<ProductVariantListResponse>> GetByProductIdAsync(int productId);
        Task<ProductVariantDetailResponse?> GetByIdAsync(int id);
        Task<ProductVariantDetailResponse?> GetBySkuAsync(string sku);
        Task<int> CreateAsync(CreateProductVariantRequest request);
        Task UpdateAsync(int id, UpdateProductVariantRequest request);
        Task DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
        Task AddStockAsync(int id, int quantity);
        Task UpdateStockQuantityAsync(int id, int quantity);
    }
}
