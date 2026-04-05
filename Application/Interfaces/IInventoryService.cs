using Application.DTOs;

namespace Application.Interfaces
{
    public interface IInventoryService
    {
        Task<List<WarehouseDto>> GetWarehousesAsync();
        Task<WarehouseDto?> GetWarehouseByIdAsync(int id);
        Task<int> CreateWarehouseAsync(CreateWarehouseRequest request);
        
        Task<List<StockEntryDto>> GetStockEntriesAsync(int? warehouseId = null);
        Task<StockEntryDto?> GetStockEntryByIdAsync(int id);
        Task<int> CreateStockEntryAsync(CreateStockEntryRequest request);
        Task ConfirmStockEntryAsync(int id);
        
        Task<int> GetStockQuantityAsync(int productId, int? warehouseId = null);
    }
}
