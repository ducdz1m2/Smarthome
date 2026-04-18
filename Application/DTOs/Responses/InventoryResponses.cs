namespace Application.DTOs.Responses
{
    // Tồn kho theo sản phẩm
    public class ProductInventoryResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        // Category info
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryPath { get; set; }

        // Brand info
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;

        // Stock info
        public int TotalQuantity { get; set; }
        public int TotalReserved { get; set; }
        public int AvailableStock => TotalQuantity - TotalReserved;
        public bool IsLowStock { get; set; }
        public int LowStockThreshold { get; set; } = 10;

        public string? MainImageUrl { get; set; }

        // Chi tiết theo từng kho
        public List<WarehouseStockDetailResponse> WarehouseStocks { get; set; } = new();

        // Chi tiết theo từng phân loại
        public List<ProductVariantInventoryResponse> Variants { get; set; } = new();
    }

    public class WarehouseStockDetailResponse
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableStock => Quantity - ReservedQuantity;
    }

    public class ProductVariantInventoryResponse
    {
        public int VariantId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
        public bool IsActive { get; set; }
        public List<WarehouseStockDetailResponse> WarehouseStocks { get; set; } = new();
        public int TotalQuantity => WarehouseStocks.Sum(w => w.Quantity);
        public int TotalReserved => WarehouseStocks.Sum(w => w.ReservedQuantity);
        public int AvailableStock => TotalQuantity - TotalReserved;
    }

    // Tồn kho tổng hợp theo danh mục
    public class CategoryInventorySummaryResponse
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryPath { get; set; }
        public int ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        
        public int TotalProducts { get; set; }
        public int TotalStockQuantity { get; set; }
        public decimal TotalStockValue { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        
        // Chi tiết theo từng kho
        public List<WarehouseCategoryStockResponse> WarehouseBreakdown { get; set; } = new();
    }

    public class WarehouseCategoryStockResponse
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int TotalQuantity { get; set; }
    }

    // Tồn kho tổng hợp theo kho
    public class WarehouseInventorySummaryResponse
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public int TotalReserved { get; set; }
        public int AvailableStock => TotalQuantity - TotalReserved;
        public decimal TotalStockValue { get; set; }
        public int LowStockCount { get; set; }
        
        // Phân loại theo danh mục trong kho này
        public List<CategoryStockInWarehouseResponse> CategoryBreakdown { get; set; } = new();
    }

    public class CategoryStockInWarehouseResponse
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int TotalQuantity { get; set; }
    }

    // Phiếu nhập kho chi tiết (khác với StockEntryResponse cũ)
    public class StockEntryDetailListResponse
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? Note { get; set; }
        public decimal TotalCost { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<StockEntryDetailItemResponse> Details { get; set; } = new();
    }

    public class StockEntryDetailItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost => Quantity * UnitCost;
        public string? CategoryName { get; set; }
    }

    public class StockEntryListItemResponse
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public bool IsCompleted { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Báo cáo tồn kho
    public class InventoryReportResponse
    {
        public DateTime ReportDate { get; set; }
        public int TotalProducts { get; set; }
        public int TotalStockQuantity { get; set; }
        public decimal TotalStockValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public List<CategoryInventorySummaryResponse> CategorySummaries { get; set; } = new();
        public List<WarehouseInventorySummaryResponse> WarehouseSummaries { get; set; } = new();
    }

    // Lịch sử giao dịch kho
    public class StockMovementResponse
    {
        public int Id { get; set; }
        public DateTime MovementDate { get; set; }
        public string MovementType { get; set; } = string.Empty; // IN, OUT, TRANSFER, ADJUST
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Note { get; set; }
        public string? CategoryName { get; set; }
    }
}
