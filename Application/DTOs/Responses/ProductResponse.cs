namespace Application.DTOs.Responses
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int StockQuantity { get; set; }
        public int FrozenStockQuantity { get; set; }
        public int AvailableStock => StockQuantity - FrozenStockQuantity;
        public string? Description { get; set; }
        public Dictionary<string, string> Specs { get; set; } = new();
        public bool IsActive { get; set; }
        public bool RequiresInstallation { get; set; }
        
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        
        public List<ProductVariantResponse> Variants { get; set; } = new();
        public List<ProductImageResponse> Images { get; set; } = new();
        public List<ProductCommentResponse> Comments { get; set; } = new();
        
        public DateTime CreatedAt { get; set; }
    }

    public class ProductCommentResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ProductCommentResponse> Replies { get; set; } = new();
    }

    public class ProductVariantResponse
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class ProductImageResponse
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsMain { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductListResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string? MainImageUrl { get; set; }
    }
}
