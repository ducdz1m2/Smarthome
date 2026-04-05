namespace Domain.Entities.Sales
{
    using Domain.Entities.Common;
    using Domain.Enums;
    using Domain.Exceptions;

    public class Warranty : BaseEntity
    {
        public int OrderItemId { get; private set; }
        public int ProductId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public WarrantyStatus Status { get; private set; } = WarrantyStatus.Active;

        public virtual ICollection<WarrantyClaim> Claims { get; private set; } = new List<WarrantyClaim>();

        private Warranty() { }

        public static Warranty Create(int orderItemId, int productId, int durationInMonths)
        {
            if (durationInMonths <= 0)
                throw new DomainException("Thời hạn bảo hành phải lớn hơn 0");

            var startDate = DateTime.UtcNow;

            return new Warranty
            {
                OrderItemId = orderItemId,
                ProductId = productId,
                StartDate = startDate,
                EndDate = startDate.AddMonths(durationInMonths),
                Status = WarrantyStatus.Active
            };
        }

        public bool IsValid(DateTime date) => date <= EndDate && Status == WarrantyStatus.Active;

        public void Extend(int additionalMonths)
        {
            EndDate = EndDate.AddMonths(additionalMonths);
        }

        public void MarkExpired()
        {
            if (DateTime.UtcNow > EndDate)
                Status = WarrantyStatus.Expired;
        }

        public WarrantyClaim CreateClaim(string issue)
        {
            if (!IsValid(DateTime.UtcNow))
                throw new DomainException("Bảo hành đã hết hạn");

            var claim = WarrantyClaim.Create(Id, issue);
            Claims.Add(claim);
            return claim;
        }
    }

    public enum WarrantyStatus
    {
        Active = 0,
        Expired = 1,
        Void = 2
    }
}
