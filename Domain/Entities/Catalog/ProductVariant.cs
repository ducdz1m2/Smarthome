namespace Domain.Entities.Catalog
{
    using System.Text.Json;
    using Domain.Entities.Common;
    using Domain.Exceptions;

    public class ProductVariant : BaseEntity
    {
        public int ProductId { get; private set; }
        public string Sku { get; private set; } = null!;
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }
        public string AttributesJson { get; private set; } = "{}";
        public bool IsActive { get; private set; } = true;

        public virtual Product Product { get; private set; } = null!;

        private ProductVariant() { }

        public static ProductVariant Create(Product product, string sku, decimal price, int initialStock, Dictionary<string, string> attributes)
        {
            var variant = new ProductVariant
            {
                ProductId = product.Id,
                Sku = sku.Trim().ToUpper(),
                Price = price,
                StockQuantity = initialStock,
                AttributesJson = JsonSerializer.Serialize(attributes),
                IsActive = true
            };

            return variant;
        }

        public static ProductVariant Create(int productId, string sku, decimal price, int initialStock, Dictionary<string, string> attributes)
        {
            var variant = new ProductVariant
            {
                ProductId = productId,
                Sku = sku.Trim().ToUpper(),
                Price = price,
                StockQuantity = initialStock,
                AttributesJson = JsonSerializer.Serialize(attributes),
                IsActive = true
            };

            return variant;
        }

        public void UpdatePrice(decimal newPrice)
        {
            Price = newPrice;
        }

        public void UpdateAttributes(Dictionary<string, string> attributes)
        {
            AttributesJson = JsonSerializer.Serialize(attributes);
        }

        public Dictionary<string, string> GetAttributes()
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(AttributesJson) ?? new();
        }

        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Số lượng thêm phải lớn hơn 0");

            StockQuantity += quantity;
        }

        public void DeductStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Số lượng trừ phải lớn hơn 0");

            if (StockQuantity < quantity)
                throw new DomainException($"Không đủ tồn kho cho variant. Tồn kho: {StockQuantity}, Yêu cầu: {quantity}");

            StockQuantity -= quantity;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public decimal GetEffectivePrice(decimal? productBasePrice)
        {
            return Price > 0 ? Price : (productBasePrice ?? 0);
        }
    }
}
