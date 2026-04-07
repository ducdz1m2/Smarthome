using Domain.Entities.Catalog;

namespace Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<Product?> GetByIdWithDetailsAsync(int id);
        Task<Product?> GetByIdForUpdateAsync(int id);
        Task<Product?> GetBySkuAsync(string sku);
        Task<List<Product>> GetAllAsync();
        Task<List<Product>> GetByCategoryAsync(int categoryId);
        Task<List<Product>> GetByBrandAsync(int brandId);
        Task<(List<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, int? categoryId = null, int? brandId = null, bool? isActive = null);
        Task<bool> ExistsAsync(string sku, int? excludeId = null);
        Task<bool> ExistsAsync(int id);
        Task AddAsync(Product product);
        void Update(Product product);
        void Delete(Product product);
        Task<int> CountAsync();
        Task SaveChangesAsync();
        Task<List<Product>> SearchAsync(string keyword, int? categoryId);
    }
}
