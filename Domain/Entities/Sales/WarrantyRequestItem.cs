namespace Domain.Entities.Sales;

using Domain.Abstractions;

/// <summary>
/// Represents an item in a warranty request with damaged product tracking.
/// </summary>
public class WarrantyRequestItem : Entity
{
    public int WarrantyRequestId { get; private set; }
    public int OrderItemId { get; private set; }
    public int Quantity { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsDamaged { get; private set; }
    public bool ReturnedToInventory { get; private set; }

    public virtual WarrantyRequest WarrantyRequest { get; private set; } = null!;

    private WarrantyRequestItem() { }

    public static WarrantyRequestItem Create(int warrantyRequestId, int orderItemId, int quantity, string description, bool isDamaged = false)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        return new WarrantyRequestItem
        {
            WarrantyRequestId = warrantyRequestId,
            OrderItemId = orderItemId,
            Quantity = quantity,
            Description = description.Trim(),
            IsDamaged = isDamaged,
            ReturnedToInventory = false
        };
    }

    public void MarkAsDamaged()
    {
        IsDamaged = true;
    }

    public void MarkAsReturnedToInventory()
    {
        if (!IsDamaged)
            throw new InvalidOperationException("Cannot mark non-damaged item as returned to inventory");

        ReturnedToInventory = true;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        Description = description.Trim();
    }
}
