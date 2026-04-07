namespace Application.DTOs.Requests
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, string>? Specs { get; set; }
        public bool RequiresInstallation { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int? SupplierId { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
}
