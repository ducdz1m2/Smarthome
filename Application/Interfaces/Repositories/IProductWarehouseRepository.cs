using Domain.Entities.Inventory;

namespace Application.Interfaces.Repositories
{
    public interface IProductWarehouseRepository
    {
        Task<ProductWarehouse?> GetByIdAsync(int id);
        Task<ProductWarehouse?> GetByProductAndWarehouseAsync(int productId, int warehouseId);
        Task<List<ProductWarehouse>> GetByProductAsync(int productId);
        Task<List<ProductWarehouse>> GetByWarehouseAsync(int warehouseId);
        Task<List<ProductWarehouse>> GetAllAsync();
        Task<List<ProductWarehouse>> GetByProductsAsync(List<int> productIds);
        Task AddAsync(ProductWarehouse productWarehouse);
        void Update(ProductWarehouse productWarehouse);
        void Delete(ProductWarehouse productWarehouse);
        Task SaveChangesAsync();
    }
}
