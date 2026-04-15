namespace Application.DTOs.Requests
{
    public class UpdateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }

        [Obsolete("StockQuantity không được cập nhật trực tiếp. Sử dụng InventoryService để quản lý tồn kho.")]
        public int StockQuantity { get; set; }

        public string? Description { get; set; }
        public Dictionary<string, string>? Specs { get; set; }
        public bool RequiresInstallation { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int? SupplierId { get; set; }
        public bool IsActive { get; set; } = true;
        public List<string>? ImageUrls { get; set; }
    }

    public class AddStockRequest
    {
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public int? WarehouseId { get; set; }
    }
}
