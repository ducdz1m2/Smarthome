namespace Domain.Entities.Catalog;

using System.Text.Json;
using Domain.Abstractions;
using Domain.Exceptions;
using Domain.ValueObjects;

public class ProductVariant : Entity
{
    public int ProductId { get; private set; }
    public Sku Sku { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public int StockQuantity { get; private set; }
    public int FrozenStockQuantity { get; private set; }
    public string AttributesJson { get; private set; } = "{}";
    public bool IsActive { get; private set; } = true;

    public virtual Product Product { get; private set; } = null!;

    private ProductVariant() { }

    public static ProductVariant Create(Product product, Sku sku, Money price, int initialStock, Dictionary<string, string> attributes)
    {
        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Sku = sku,
            Price = price,
            StockQuantity = initialStock,
            AttributesJson = JsonSerializer.Serialize(attributes),
            IsActive = true
        };

        return variant;
    }

    public static ProductVariant Create(int productId, Sku sku, Money price, int initialStock, Dictionary<string, string> attributes)
    {
        var variant = new ProductVariant
        {
            ProductId = productId,
            Sku = sku,
            Price = price,
            StockQuantity = initialStock,
            AttributesJson = JsonSerializer.Serialize(attributes),
            IsActive = true
        };

        return variant;
    }

    // Legacy overloads for backward compatibility
    public static ProductVariant Create(Product product, string sku, decimal price, int initialStock, Dictionary<string, string> attributes)
    {
        return Create(product, Sku.Create(sku), Money.Vnd(price), initialStock, attributes);
    }

    public static ProductVariant Create(int productId, string sku, decimal price, int initialStock, Dictionary<string, string> attributes)
    {
        return Create(productId, Sku.Create(sku), Money.Vnd(price), initialStock, attributes);
    }

    public void UpdatePrice(Money newPrice)
    {
        Price = newPrice;
    }

    public void UpdatePrice(decimal newPrice)
    {
        UpdatePrice(Money.Vnd(newPrice));
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
                throw new InvalidQuantityException(quantity, "AddStock");

            StockQuantity += quantity;
        }

        public void ReserveStock(int quantity)
        {
            if (quantity <= 0)
                throw new InvalidQuantityException(quantity, "ReserveStock");

            if (GetAvailableStock() < quantity)
                throw new InsufficientStockException(Id, 0, quantity, GetAvailableStock());

            FrozenStockQuantity += quantity;
        }

        public void ReleaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new InvalidQuantityException(quantity, "ReleaseStock");

            FrozenStockQuantity = Math.Max(0, FrozenStockQuantity - quantity);
        }

        public void DeductStock(int quantity)
        {
            if (quantity <= 0)
                throw new InvalidQuantityException(quantity, "DeductStock");

            if (StockQuantity < quantity)
                throw new InsufficientStockException(Id, 0, quantity, StockQuantity);

            StockQuantity -= quantity;
            FrozenStockQuantity = Math.Max(0, FrozenStockQuantity - quantity);
        }

        public int GetAvailableStock() => StockQuantity - FrozenStockQuantity;


        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public Money GetEffectivePrice(Money? productBasePrice)
        {
            return Price.Amount > 0 ? Price : (productBasePrice ?? Money.Zero());
        }

        public decimal GetEffectivePriceAmount(decimal? productBasePrice)
        {
            return GetEffectivePrice(productBasePrice.HasValue ? Money.Vnd(productBasePrice.Value) : null).Amount;
        }
    }
