namespace Domain.Entities.Catalog;

using System.Text.Json;
using Domain.Abstractions;
using Domain.Entities.Inventory;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// Product aggregate root - represents a product in the catalog.
/// </summary>
public class Product : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public Sku Sku { get; private set; } = null!;
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
    public virtual ICollection<Domain.Entities.Promotions.PromotionProduct> PromotionProducts { get; private set; } = new List<Domain.Entities.Promotions.PromotionProduct>();

    private Product() { }

    public static Product Create(string name, string sku, int categoryId, int brandId, int? supplierId = null, bool requiresInstallation = false)
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
            Sku = Sku.Create(sku),
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

        public void Update(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tên sản phẩm không được trống", nameof(name));

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

        
        public ProductVariant AddVariant(string sku, Money price, Dictionary<string, string> attributes)
        {
            var variant = ProductVariant.Create(this, Sku.Create(sku), price, 0, attributes);
            Variants.Add(variant);
            return variant;
        }

        // Legacy overload for backward compatibility
        public ProductVariant AddVariant(string sku, decimal price, Dictionary<string, string> attributes)
        {
            return AddVariant(sku, Money.Vnd(price), attributes);
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

        public void SetRequiresInstallation(bool requiresInstallation)
        {
            RequiresInstallation = requiresInstallation;
        }

        public void SetStockQuantity(int quantity)
        {
            if (quantity < 0)
                throw new InvalidQuantityException(quantity, "SetStockQuantity");
            
            StockQuantity = quantity;
        }

        public void ChangeSupplier(int supplierId)
        {
            if (supplierId <= 0)
                throw new ValidationException(nameof(supplierId), "SupplierId không hợp lệ");
            
            SupplierId = supplierId;
        }

        public void RemoveSupplier()
        {
            SupplierId = null;
        }

        //kho

        public void AddStockToVariant(string sku, int quantity)
        {
            var variant = Variants.FirstOrDefault(v => v.Sku.Value == sku);
            if (variant == null) throw new DomainException($"Không tìm thấy SKU: {sku}");

            variant.AddStock(quantity);

           
            SyncTotalStock();

      
            AddDomainEvent(new ProductStockSynchronizedEvent(Id, variant.Id, StockQuantity));
        }

        public void ReserveStockForVariant(string sku, int quantity)
        {
            var variant = Variants.FirstOrDefault(v => v.Sku.Value == sku);
            if (variant == null) throw new DomainException($"Không tìm thấy SKU: {sku}");

            variant.ReserveStock(quantity);

            SyncTotalStock();
        }

     
        private void SyncTotalStock()
        {
            StockQuantity = Variants.Sum(v => v.StockQuantity);
            FrozenStockQuantity = Variants.Sum(v => v.FrozenStockQuantity);
        }
    }
