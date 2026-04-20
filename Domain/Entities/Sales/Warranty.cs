namespace Domain.Entities.Sales;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Exceptions;

/// <summary>
/// Warranty aggregate root - represents a product warranty for a specific product instance.
/// </summary>
public class Warranty : AggregateRoot
{
    public int ProductId { get; private set; }
    public int? VariantId { get; private set; }
    public int OrderItemId { get; private set; } // Link to OrderItem instead of serial number
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public WarrantyStatus Status { get; private set; } = WarrantyStatus.Active;
    public int DurationMonths { get; private set; }
    public int? InstalledByTechnicianId { get; private set; }
    public int ClaimsCount { get; private set; } = 0;

    public virtual ICollection<WarrantyClaim> Claims { get; private set; } = new List<WarrantyClaim>();

    private Warranty() { }

    public static Warranty Create(int productId, int? variantId, int orderItemId, int durationMonths)
    {
        if (durationMonths <= 0)
            throw new ValidationException(nameof(durationMonths), "Thời hạn bảo hành phải lớn hơn 0");

        return new Warranty
        {
            ProductId = productId,
            VariantId = variantId,
            OrderItemId = orderItemId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(durationMonths),
            DurationMonths = durationMonths,
            Status = WarrantyStatus.Active
        };
    }

    public bool IsValid(DateTime date) => date <= EndDate && Status == WarrantyStatus.Active;

    public void Extend(int additionalMonths)
    {
        if (additionalMonths <= 0)
            throw new ValidationException(nameof(additionalMonths), "Thời gian gia hạn phải lớn hơn 0");

        EndDate = EndDate.AddMonths(additionalMonths);
        DurationMonths += additionalMonths;
    }

    public void MarkExpired()
    {
        if (DateTime.UtcNow > EndDate)
            Status = WarrantyStatus.Expired;
    }

    public void Void()
    {
        Status = WarrantyStatus.Void;
    }

    public void SetStatus(WarrantyStatus status)
    {
        if (status == WarrantyStatus.Void && Claims.Any(c => c.Status == WarrantyClaimStatus.ReplacementApproved))
            throw new BusinessRuleViolationException("WarrantyHasApprovedClaims", "Không thể vô hiệu hóa bảo hành khi có khiếu nại đã được duyệt");

        Status = status;
    }

    public void SetInstalledByTechnicianId(int technicianId)
    {
        InstalledByTechnicianId = technicianId;
    }

    public WarrantyClaim CreateClaim(string issue)
    {
        if (!IsValid(DateTime.UtcNow))
            throw new BusinessRuleViolationException("WarrantyExpired", "Bảo hành đã hết hạn");

        var claim = WarrantyClaim.Create(Id, ProductId, VariantId, OrderItemId, issue);
        Claims.Add(claim);
        ClaimsCount++;
        return claim;
    }
}

public enum WarrantyStatus
{
    Active = 0,
    Expired = 1,
    Void = 2
}
