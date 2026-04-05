namespace Application.DTOs.Requests
{
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
