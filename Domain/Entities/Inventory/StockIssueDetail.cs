namespace Domain.Entities.Inventory;

using Domain.Abstractions;
using Domain.Exceptions;

/// <summary>
/// StockIssueDetail entity - represents a line item in a stock issue.
/// </summary>
public class StockIssueDetail : Entity
{
    public int StockIssueId { get; set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int? VariantId { get; private set; }

    // Navigation property removed to prevent EF Core tracking conflicts
    // public virtual StockIssue StockIssue { get; private set; } = null!;

    private StockIssueDetail() { }

    public static StockIssueDetail Create(int productId, int quantity, int? variantId = null)
    {
        if (productId <= 0)
            throw new ValidationException(nameof(productId), "ProductId không hợp lệ");

        if (quantity <= 0)
            throw new ValidationException(nameof(quantity), "Quantity không hợp lệ");

        return new StockIssueDetail
        {
            ProductId = productId,
            Quantity = quantity,
            VariantId = variantId
        };
    }
}
