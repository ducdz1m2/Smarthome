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
    public int? WarrantyId { get; private set; }
    public int ProductId { get; private set; }
    public int? VariantId { get; private set; }
    public int OrderItemId { get; private set; } // Link to OrderItem instead of serial number
    public int OrderId { get; private set; } // Direct link to Order for easier lookup
    public int? InstallationBookingId { get; private set; }
    public int? AssignedTechnicianId { get; private set; }
    public WarrantyType WarrantyType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public WarrantyRequestStatus Status { get; private set; } = WarrantyRequestStatus.Pending;
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? TechnicianNotes { get; private set; }

    public virtual Warranty? Warranty { get; private set; }
    public virtual ICollection<WarrantyRequestItem> Items { get; private set; } = new List<WarrantyRequestItem>();

    private WarrantyRequest() { }

    public static WarrantyRequest Create(int? warrantyId, int productId, int? variantId, int orderItemId, int orderId, WarrantyType warrantyType, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException(nameof(description), "Mô tả yêu cầu bảo hành không được trống");

        return new WarrantyRequest
        {
            WarrantyId = warrantyId,
            ProductId = productId,
            VariantId = variantId,
            OrderItemId = orderItemId,
            OrderId = orderId,
            WarrantyType = warrantyType,
            Description = description.Trim(),
            Status = WarrantyRequestStatus.Pending
        };
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

    public void LinkToInstallationBooking(int bookingId)
    {
        InstallationBookingId = bookingId;
    }

    public void AssignTechnician(int technicianId)
    {
        AssignedTechnicianId = technicianId;
    }
}
