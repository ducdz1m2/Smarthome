using Domain.Entities.Catalog;

namespace Domain.Specifications;

/// <summary>
/// Specification for active products.
/// </summary>
public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification()
    {
        Criteria = p => p.IsActive;
        AddInclude(p => p.Category);
        AddInclude(p => p.Brand);
        ApplyOrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification for products in a category.
/// </summary>
public class ProductsByCategorySpecification : BaseSpecification<Product>
{
    public ProductsByCategorySpecification(int categoryId)
    {
        Criteria = p => p.CategoryId == categoryId && p.IsActive;
        AddInclude(p => p.Brand);
        ApplyOrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification for products with low stock.
/// </summary>
public class LowStockProductsSpecification : BaseSpecification<Product>
{
    public LowStockProductsSpecification(int threshold)
    {
        Criteria = p => p.StockQuantity <= threshold;
        ApplyOrderBy(p => p.StockQuantity);
    }
}

/// <summary>
/// Specification for searching products.
/// </summary>
public class ProductSearchSpecification : BaseSpecification<Product>
{
    public ProductSearchSpecification(string searchTerm)
    {
        Criteria = p =>
            p.IsActive &&
            (p.Name.Contains(searchTerm) ||
             p.Description != null && p.Description.Contains(searchTerm));
        AddInclude(p => p.Category);
        AddInclude(p => p.Brand);
    }
}

/// <summary>
/// Specification for products requiring installation.
/// </summary>
public class ProductsRequiringInstallationSpecification : BaseSpecification<Product>
{
    public ProductsRequiringInstallationSpecification()
    {
        Criteria = p => p.RequiresInstallation && p.IsActive;
    }
}
