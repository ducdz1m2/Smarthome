namespace Domain.Entities.Sales
{
    using Domain.Entities.Common;
    using Domain.Enums;
    using Domain.Exceptions;

    public class WarrantyClaim : BaseEntity
    {
        public int WarrantyId { get; private set; }
        public DateTime ClaimDate { get; private set; }
        public string Issue { get; private set; } = string.Empty;
        public string? Resolution { get; private set; }
        public WarrantyClaimStatus Status { get; private set; } = WarrantyClaimStatus.Pending;
        public int? TechnicianId { get; private set; }

        public virtual Warranty? Warranty { get; private set; }

        private WarrantyClaim() { }

        public static WarrantyClaim Create(int warrantyId, string issue)
        {
            if (string.IsNullOrWhiteSpace(issue))
                throw new DomainException("Mô tả lỗi không được trống");

            return new WarrantyClaim
            {
                WarrantyId = warrantyId,
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
}
