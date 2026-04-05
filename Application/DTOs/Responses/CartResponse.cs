namespace Application.DTOs.Responses
{
    public class CartItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public bool RequiresInstallation { get; set; }
        public int StockAvailable { get; set; }
    }

    public class CartResponse
    {
        public int UserId { get; set; }
        public List<CartItemResponse> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public int TotalItems { get; set; }
    }

    

    
}
