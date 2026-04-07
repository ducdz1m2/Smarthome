namespace Application.DTOs.Requests
{
    public class CompleteStockEntryRequest
    {
        public int StockEntryId { get; set; }
    }

    public class AdjustStockRequest
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Adjustment { get; set; }
        public string? Reason { get; set; }
    }

    public class TransferStockRequest
    {
        public int ProductId { get; set; }
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }

    public class InventoryFilterRequest
    {
        public int? WarehouseId { get; set; }
        public int? CategoryId { get; set; }
        public int? ProductId { get; set; }
        public bool? LowStockOnly { get; set; }
        public string? SearchTerm { get; set; }
    }
}
