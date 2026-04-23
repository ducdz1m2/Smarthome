using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<List<ProductListResponse>> GetAllAsync();
        Task<ProductResponse?> GetByIdAsync(int id);
        Task<ProductResponse?> GetBySkuAsync(string sku);
        Task<(List<ProductListResponse> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, int? categoryId = null, int? brandId = null, bool? isActive = null, int? promotionId = null);
        Task<List<ProductListResponse>> GetByCategoryAsync(int categoryId);
        Task<List<ProductListResponse>> SearchAsync(string keyword, string? filters);
        Task<int> CreateAsync(CreateProductRequest request);
        Task UpdateAsync(int id, UpdateProductRequest request);
        Task DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
        Task AddStockAsync(int id, AddStockRequest request);
        Task<bool> UpdateStockAsync(int productId, int quantity);
    }
}
