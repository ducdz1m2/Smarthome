namespace Domain.Events
{
    public class WarrantyClaimCreatedEvent : DomainEvent
    {
        public int ClaimId { get; }
        public int WarrantyId { get; }
        public int ProductId { get; }
        public string Issue { get; }

        public WarrantyClaimCreatedEvent(int claimId, int warrantyId, int productId, string issue)
        {
            ClaimId = claimId;
            WarrantyId = warrantyId;
            ProductId = productId;
            Issue = issue;
        }
    }
}
