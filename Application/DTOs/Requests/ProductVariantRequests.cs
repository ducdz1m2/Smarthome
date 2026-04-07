namespace Application.DTOs.Requests
{
    public class CreateProductVariantRequest
    {
        public int ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
    }

    public class UpdateProductVariantRequest
    {
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    public class AddVariantStockRequest
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public string? Reason { get; set; }
    }
}
