namespace Domain.Entities.Sales;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Exceptions;

/// <summary>
/// WarrantyClaim entity - represents a warranty claim for a product instance.
/// </summary>
public class WarrantyClaim : Entity
{
    public int WarrantyId { get; private set; }
    public int ProductId { get; private set; }
    public int? VariantId { get; private set; }
    public int OrderItemId { get; private set; } // Link to OrderItem instead of serial number
    public DateTime ClaimDate { get; private set; }
    public string Issue { get; private set; } = string.Empty;
    public string? Resolution { get; private set; }
    public WarrantyClaimStatus Status { get; private set; } = WarrantyClaimStatus.Pending;
    public int? TechnicianId { get; private set; }
    public int? WarrantyRequestId { get; private set; } // Link to warranty request if applicable

    public virtual Warranty? Warranty { get; private set; }

    private WarrantyClaim() { }

    public static WarrantyClaim Create(int warrantyId, int productId, int? variantId, int orderItemId, string issue)
    {
        if (string.IsNullOrWhiteSpace(issue))
            throw new ValidationException(nameof(issue), "Mô tả lỗi không được trống");

        return new WarrantyClaim
        {
            WarrantyId = warrantyId,
            ProductId = productId,
            VariantId = variantId,
            OrderItemId = orderItemId,
            ClaimDate = DateTime.UtcNow,
            Issue = issue.Trim(),
            Status = WarrantyClaimStatus.Pending
        };
    }

    public void AssignTechnician(int technicianId)
    {
        TechnicianId = technicianId;
        Status = WarrantyClaimStatus.Assigned;
    }

    public void Resolve(string resolution, bool isApproved)
    {
        Resolution = resolution?.Trim();
        Status = isApproved ? WarrantyClaimStatus.Resolved : WarrantyClaimStatus.Rejected;
    }

    public void ApproveReplacement()
    {
        Resolution = "Đổi sản phẩm mới";
        Status = WarrantyClaimStatus.ReplacementApproved;
    }

    public void LinkToWarrantyRequest(int warrantyRequestId)
    {
        WarrantyRequestId = warrantyRequestId;
    }
}

public enum WarrantyClaimStatus
{
    Pending = 0,
    Assigned = 1,
    InProgress = 2,
    Resolved = 3,
    Rejected = 4,
    ReplacementApproved = 5
}
