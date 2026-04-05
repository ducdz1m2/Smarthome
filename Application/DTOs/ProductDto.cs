namespace Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public bool RequiresInstallation { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public int? SupplierId { get; set; }
    }

    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int? SupplierId { get; set; }
        public string? Description { get; set; }
        public bool RequiresInstallation { get; set; }
    }

    public class UpdateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
