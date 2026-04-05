namespace Application.DTOs
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateWarehouseRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? AddressStreet { get; set; }
        public string? AddressWard { get; set; }
        public string? AddressDistrict { get; set; }
        public string? AddressCity { get; set; }
        public string? Phone { get; set; }
    }

    public class StockEntryDto
    {
        public int Id { get; set; }
        public string EntryNumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<StockEntryDetailDto> Details { get; set; } = new();
    }

    public class StockEntryDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateStockEntryRequest
    {
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime EntryDate { get; set; }
        public List<CreateStockEntryDetailRequest> Details { get; set; } = new();
    }

    public class CreateStockEntryDetailRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
