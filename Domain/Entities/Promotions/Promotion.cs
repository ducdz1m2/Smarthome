namespace Domain.Entities.Promotions
{
    using Domain.Entities.Common;
    using Domain.Exceptions;
    using Domain.ValueObjects;

    public class Promotion : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public Percentage DiscountPercent { get; private set; } = null!;
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public Money? MinOrderAmount { get; private set; }
        public bool IsActive { get; private set; } = true;
        public int Priority { get; private set; } = 0;

        public virtual ICollection<PromotionProduct> PromotionProducts { get; private set; } = new List<PromotionProduct>();

        private Promotion() { }

        public static Promotion Create(string name, Percentage discountPercent, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên chương trình không được trống");

            if (endDate <= startDate)
                throw new DomainException("Ngày kết thúc phải sau ngày bắt đầu");

            return new Promotion
            {
                Name = name.Trim(),
                DiscountPercent = discountPercent,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true,
                Priority = 0
            };
        }

        public void AddProduct(int productId, Percentage? customDiscount = null)
        {
            var pp = PromotionProduct.Create(Id, productId, customDiscount);
            PromotionProducts.Add(pp);
        }

        public bool IsActiveNow()
        {
            var now = DateTime.UtcNow;
            return IsActive && now >= StartDate && now <= EndDate;
        }

        public bool AppliesTo(int productId)
        {
            return !PromotionProducts.Any() || PromotionProducts.Any(pp => pp.ProductId == productId);
        }

        public Money CalculateDiscount(Money originalPrice, int? productId = null)
        {
            if (!IsActiveNow())
                return Money.Vnd(0);

            if (productId.HasValue && !AppliesTo(productId.Value))
                return Money.Vnd(0);

            Percentage effectiveDiscount;

            if (productId.HasValue)
            {
                var pp = PromotionProducts.FirstOrDefault(p => p.ProductId == productId.Value);
                effectiveDiscount = pp?.GetEffectiveDiscount(DiscountPercent) ?? DiscountPercent;
            }
            else
            {
                effectiveDiscount = DiscountPercent;
            }

            return originalPrice.ApplyDiscount(effectiveDiscount);
        }

        public void UpdatePeriod(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new DomainException("Ngày kết thúc phải sau ngày bắt đầu");

            StartDate = start;
            EndDate = end;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
