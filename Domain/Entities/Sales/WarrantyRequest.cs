namespace Domain.Entities.Sales;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// WarrantyRequest aggregate root - represents a product warranty service request.
/// </summary>
public class WarrantyRequest : AggregateRoot
{
    public int OrderId { get; private set; }
    public WarrantyType WarrantyType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public WarrantyRequestStatus Status { get; private set; } = WarrantyRequestStatus.Pending;
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? TechnicianNotes { get; private set; }

    public virtual ICollection<WarrantyRequestItem> Items { get; private set; } = new List<WarrantyRequestItem>();

    private WarrantyRequest() { }

    public static WarrantyRequest Create(int orderId, WarrantyType warrantyType, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException(nameof(description), "Mô tả yêu cầu bảo hành không được trống");

        return new WarrantyRequest
        {
            OrderId = orderId,
            WarrantyType = warrantyType,
            Description = description.Trim(),
            Status = WarrantyRequestStatus.Pending
        };
    }

    public void AddItem(int orderItemId, int quantity, string itemDescription, bool isDamaged = false)
    {
        if (Status != WarrantyRequestStatus.Pending)
            throw new BusinessRuleViolationException("WarrantyRequestStatus", "Không thể thêm sản phẩm vào yêu cầu đã xử lý");

        var item = WarrantyRequestItem.Create(Id, orderItemId, quantity, itemDescription, isDamaged);
        Items.Add(item);
    }

    public void Approve()
    {
        if (Status != WarrantyRequestStatus.Pending)
            throw new BusinessRuleViolationException("WarrantyRequestStatus", "Chỉ có thể duyệt yêu cầu đang chờ");

        Status = WarrantyRequestStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != WarrantyRequestStatus.Pending)
            throw new BusinessRuleViolationException("WarrantyRequestStatus", "Chỉ có thể từ chối yêu cầu đang chờ");

        Status = WarrantyRequestStatus.Rejected;
        TechnicianNotes = reason;
    }

    public void Start()
    {
        if (Status != WarrantyRequestStatus.Approved)
            throw new BusinessRuleViolationException("WarrantyRequestStatus", "Chỉ có thể bắt đầu yêu cầu đã duyệt");

        Status = WarrantyRequestStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(string? technicianNotes = null)
    {
        if (Status != WarrantyRequestStatus.InProgress)
            throw new BusinessRuleViolationException("WarrantyRequestStatus", "Chỉ có thể hoàn thành yêu cầu đang thực hiện");

        Status = WarrantyRequestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        TechnicianNotes = technicianNotes;
    }
}

public class WarrantyRequestItem : Entity
{
    public int WarrantyRequestId { get; private set; }
    public int OrderItemId { get; private set; }
    public int Quantity { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsDamaged { get; private set; } = false;
    public bool ReturnedToInventory { get; private set; } = false;

    private WarrantyRequestItem() { }

    public static WarrantyRequestItem Create(int warrantyRequestId, int orderItemId, int quantity, string description, bool isDamaged = false)
    {
        return new WarrantyRequestItem
        {
            WarrantyRequestId = warrantyRequestId,
            OrderItemId = orderItemId,
            Quantity = quantity,
            Description = description,
            IsDamaged = isDamaged
        };
    }

    public void MarkAsReturnedToInventory()
    {
        ReturnedToInventory = true;
    }
}
