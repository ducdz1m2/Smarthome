using Domain.Entities.Catalog;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Product aggregate.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByBrandAsync(int brandId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetBySupplierAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        int? categoryId = null,
        int? brandId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsBySkuAsync(string sku, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasStockInWarehouseAsync(int productId, int warehouseId, int requiredQuantity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchAsync(string keyword, int? categoryId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Category entity.
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdWithChildrenAsync(int id, CancellationToken cancellationToken = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetChildrenAsync(int parentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Brand entity.
/// </summary>
public interface IBrandRepository : IRepository<Brand>
{
    Task<Brand?> GetByIdWithProductsAsync(int id, CancellationToken cancellationToken = default);
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for ProductVariant entity.
/// </summary>
public interface IProductVariantRepository : IRepository<ProductVariant>
{
    Task<ProductVariant?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductVariant>> GetByProductAsync(int productId, CancellationToken cancellationToken = default);
}
