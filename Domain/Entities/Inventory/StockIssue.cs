namespace Domain.Entities.Inventory;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;

/// <summary>
/// StockIssue entity - represents stock dispatch/issue for installation or other purposes.
/// </summary>
public class StockIssue : AggregateRoot
{
    public DateTime IssueDate { get; private set; }
    public int WarehouseId { get; private set; }
    public int? BookingId { get; private set; }
    public StockIssueType IssueType { get; private set; }
    public string? Note { get; private set; }
    public int? IssuedBy { get; private set; }

    // Navigation properties removed to prevent EF Core tracking conflicts
    // public virtual Warehouse Warehouse { get; private set; } = null!;
    // public virtual Installation.InstallationBooking? Booking { get; private set; }
    // public virtual ICollection<StockIssueDetail> Details { get; private set; } = new List<StockIssueDetail>();

    private StockIssue() { }

    public static StockIssue Create(int warehouseId, StockIssueType issueType, int? bookingId = null, int? issuedBy = null, string? note = null)
    {
        if (warehouseId <= 0)
            throw new ValidationException(nameof(warehouseId), "WarehouseId không hợp lệ");

        return new StockIssue
        {
            IssueDate = DateTime.UtcNow,
            WarehouseId = warehouseId,
            BookingId = bookingId,
            IssueType = issueType,
            IssuedBy = issuedBy,
            Note = note?.Trim()
        };
    }

    public StockIssueDetail AddItem(int productId, int quantity, int? variantId = null)
    {
        var detail = StockIssueDetail.Create(productId, quantity, variantId: variantId);
        // Details.Add(detail); // Commented out since we removed the navigation property
        return detail;
    }

    public void Complete()
    {
        // Since we removed the Details navigation property, we need to pass the items separately
        // For now, we'll just add a domain event without detail information
        // In a real implementation, you might want to track the items separately or pass them as parameters
    }

    public void CompleteWithItems(IEnumerable<StockIssueDetail> items)
    {
        foreach (var detail in items)
        {
            AddDomainEvent(new StockDispatchedEvent(Id, detail.ProductId, WarehouseId, detail.Quantity, BookingId));
        }
    }
}
