namespace Domain.Entities.Catalog
{
    using System.Text.Json;
    using Domain.Entities.Common;
    using Domain.Entities.Inventory;
    using Domain.Enums;
    using Domain.Events;
    using Domain.Exceptions;
    using Domain.ValueObjects;

    public class Product : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public Sku Sku { get; private set; } = null!;
        public Money BasePrice { get; private set; } = null!;
        public int StockQuantity { get; private set; }
        public int FrozenStockQuantity { get; private set; }
        public string? Description { get; private set; }
        public string SpecsJson { get; private set; } = "{}";
        public bool IsActive { get; private set; } = true;
        public bool RequiresInstallation { get; private set; }
        public int CategoryId { get; private set; }
        public int BrandId { get; private set; }
        public int? SupplierId { get; private set; }

        public virtual Category Category { get; private set; } = null!;
        public virtual Brand Brand { get; private set; } = null!;
        public virtual Supplier? Supplier { get; private set; }
        public virtual ICollection<ProductVariant> Variants { get; private set; } = new List<ProductVariant>();
        public virtual ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();
        public virtual ICollection<ProductComment> Comments { get; private set; } = new List<ProductComment>();

        private Product() { }

        public static Product Create(string name, Sku sku, Money basePrice, int categoryId, int brandId, int? supplierId = null, bool requiresInstallation = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException(nameof(name), "Tên sản phẩm không được trống");

            if (categoryId <= 0)
                throw new ValidationException(nameof(categoryId), "CategoryId không hợp lệ");

            if (brandId <= 0)
                throw new ValidationException(nameof(brandId), "BrandId không hợp lệ");

            var product = new Product
            {
                Name = name.Trim(),
                Sku = sku,
                BasePrice = basePrice,
                CategoryId = categoryId,
                BrandId = brandId,
                SupplierId = supplierId,
                RequiresInstallation = requiresInstallation,
                StockQuantity = 0,
                FrozenStockQuantity = 0,
                SpecsJson = "{}"
            };

            product.AddDomainEvent(new ProductCreatedEvent(product.Id));
            return product;
        }

        public void Update(string name, Money basePrice, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tên sản phẩm không được trống", nameof(name));

            if (basePrice.Amount != BasePrice.Amount)
            {
                var oldPrice = BasePrice;
                BasePrice = basePrice;
                AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice.Amount, basePrice.Amount));
            }

            Name = name.Trim();
            Description = description?.Trim();
        }

        public void UpdateSpecs(Dictionary<string, string> specs)
        {
            SpecsJson = JsonSerializer.Serialize(specs);
        }

        public Dictionary<string, string> GetSpecs()
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(SpecsJson) ?? new();
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
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

        public ProductVariant AddVariant(string sku, Money price, Dictionary<string, string> attributes)
        {
            var variant = ProductVariant.Create(this, Sku.Create(sku), price, 0, attributes);
            Variants.Add(variant);
            return variant;
        }

        public ProductImage AddImage(string url, bool isMain = false, int sortOrder = 0)
        {
            if (isMain)
            {
                foreach (var img in Images.Where(i => i.IsMain))
                    img.SetAsSecondary();
            }

            var image = ProductImage.Create(Id, url, null, isMain, sortOrder);
            Images.Add(image);
            return image;
        }

        public void MoveToCategory(int newCategoryId)
        {
            if (newCategoryId <= 0)
                throw new ValidationException(nameof(newCategoryId), "CategoryId không hợp lệ");

            CategoryId = newCategoryId;
        }

        public void ChangeBrand(int newBrandId)
        {
            if (newBrandId <= 0)
                throw new ValidationException(nameof(newBrandId), "BrandId không hợp lệ");

            BrandId = newBrandId;
        }

        public Money GetEffectivePrice(DiscountType discountType, decimal discountValue)
        {
            return discountType switch
            {
                DiscountType.FixedAmount => BasePrice.Subtract(Money.Vnd(discountValue)),
                DiscountType.Percentage => BasePrice.ApplyDiscount(discountValue),
                _ => BasePrice
            };
        }
    }
}
