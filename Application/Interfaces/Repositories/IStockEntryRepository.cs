using Domain.Entities.Inventory;

namespace Application.Interfaces.Repositories
{
    public interface IStockEntryRepository
    {
        Task<StockEntry?> GetByIdAsync(int id);
        Task<StockEntry?> GetByIdWithDetailsAsync(int id);
        Task<List<StockEntry>> GetAllAsync();
        Task<List<StockEntry>> GetByWarehouseAsync(int warehouseId);
        Task<List<StockEntry>> GetBySupplierAsync(int supplierId);
        Task<List<StockEntry>> GetFilteredAsync(int? warehouseId, int? supplierId, bool? isCompleted);
        Task AddAsync(StockEntry stockEntry);
        void Update(StockEntry stockEntry);
        void Delete(StockEntry stockEntry);
        Task SaveChangesAsync();
    }
}
