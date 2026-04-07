using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IInventoryService
    {
        // Quản lý tồn kho sản phẩm
        Task<List<ProductInventoryResponse>> GetProductInventoryAsync(InventoryFilterRequest? filter = null);
        Task<ProductInventoryResponse?> GetProductInventoryByIdAsync(int productId, int? warehouseId = null);
        Task<List<ProductInventoryResponse>> GetLowStockProductsAsync(int? warehouseId = null, int threshold = 10);
        Task<List<ProductInventoryResponse>> GetOutOfStockProductsAsync(int? warehouseId = null);
        
        // Báo cáo tổng hợp theo danh mục (liên kết trực tiếp với Category)
        Task<List<CategoryInventorySummaryResponse>> GetInventoryByCategoryAsync(int? parentCategoryId = null);
        Task<CategoryInventorySummaryResponse?> GetCategoryInventorySummaryAsync(int categoryId, int? warehouseId = null);
        
        // Báo cáo tổng hợp theo kho
        Task<List<WarehouseInventorySummaryResponse>> GetInventoryByWarehouseAsync();
        Task<WarehouseInventorySummaryResponse?> GetWarehouseInventorySummaryAsync(int warehouseId);
        
        // Quản lý phiếu nhập kho (Stock Entry)
        Task<List<StockEntryListItemResponse>> GetStockEntriesAsync(int? warehouseId = null, int? supplierId = null, bool? isCompleted = null);
        Task<StockEntryDetailListResponse?> GetStockEntryByIdAsync(int id);
        Task<int> CreateStockEntryAsync(CreateStockEntryRequest request);
        Task CompleteStockEntryAsync(int stockEntryId);
        Task CancelStockEntryAsync(int stockEntryId);
        
        // Điều chỉnh tồn kho
        Task AdjustStockAsync(AdjustStockRequest request);
        Task TransferStockAsync(TransferStockRequest request);
        
        // Báo cáo tổng hợp
        Task<InventoryReportResponse> GetInventoryReportAsync();
        Task<List<StockMovementResponse>> GetStockMovementsAsync(int? productId = null, int? warehouseId = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
