using Domain.Entities.Catalog;

namespace Application.Interfaces.Repositories
{
    public interface IProductVariantRepository
    {
        Task<ProductVariant?> GetByIdAsync(int id);
        Task<ProductVariant?> GetBySkuAsync(string sku);
        Task<List<ProductVariant>> GetByProductIdAsync(int productId);
        Task<bool> ExistsAsync(string sku, int? excludeId = null);
        Task AddAsync(ProductVariant variant);
        void Update(ProductVariant variant);
        void Delete(ProductVariant variant);
        Task SaveChangesAsync();
    }
}
