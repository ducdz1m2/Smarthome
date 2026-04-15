using Domain.Entities.Inventory;

namespace Application.Interfaces.Repositories
{
    public interface IProductWarehouseRepository
    {
        Task<ProductWarehouse?> GetByIdAsync(int id);
        Task<ProductWarehouse?> GetByProductAndWarehouseAsync(int productId, int warehouseId);
        Task<ProductWarehouse?> GetByProductVariantAndWarehouseAsync(int productId, int? variantId, int warehouseId);
        Task<List<ProductWarehouse>> GetByProductAsync(int productId);
        Task<List<ProductWarehouse>> GetAvailableWarehousesForProductAsync(int productId);
        Task<List<ProductWarehouse>> GetAvailableWarehousesForProductVariantAsync(int productId, int? variantId);
        Task<List<ProductWarehouse>> GetByWarehouseAsync(int warehouseId);
        Task<List<ProductWarehouse>> GetAllAsync();
        Task<List<ProductWarehouse>> GetByProductsAsync(List<int> productIds);
        Task AddAsync(ProductWarehouse productWarehouse);
        void Update(ProductWarehouse productWarehouse);
        void Delete(ProductWarehouse productWarehouse);
        Task SaveChangesAsync();
    }
}
