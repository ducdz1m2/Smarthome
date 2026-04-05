namespace Domain.Entities.Promotions
{
    using Domain.Entities.Common;
    using Domain.Enums;
    using Domain.Exceptions;
    using Domain.ValueObjects;

    public class Coupon : BaseEntity
    {
        public string Code { get; private set; } = string.Empty;
        public DiscountType DiscountType { get; private set; }
        public decimal DiscountValue { get; private set; }
        public Money? MinOrderAmount { get; private set; }
        public Money? MaxDiscountAmount { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public int MaxUsage { get; private set; }
        public int UsedCount { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Coupon() { }

        public static Coupon Create(string code, DiscountType type, decimal value, DateTime expiry, int maxUsage = 100)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ValidationException(nameof(code), "Mã coupon không được trống");

            if (value <= 0)
                throw new ValidationException(nameof(value), "Giá trị giảm giá phải lớn hơn 0");

            if (expiry <= DateTime.UtcNow)
                throw new ValidationException(nameof(expiry), "Ngày hết hạn phải trong tương lai");

            return new Coupon
            {
                Code = code.Trim().ToUpper(),
                DiscountType = type,
                DiscountValue = value,
                ExpiryDate = expiry,
                MaxUsage = maxUsage,
                UsedCount = 0,
                IsActive = true
            };
        }

        public bool IsValid(Money orderAmount)
        {
            if (!IsActive) return false;
            if (DateTime.UtcNow > ExpiryDate) return false;
            if (UsedCount >= MaxUsage) return false;
            if (MinOrderAmount != null && orderAmount.Amount < MinOrderAmount.Amount) return false;

            return true;
        }

        public Money CalculateDiscount(Money orderAmount)
        {
            if (!IsValid(orderAmount))
                throw new CouponExpiredException(Code, ExpiryDate);

            Money discount;

            if (DiscountType == DiscountType.FixedAmount)
            {
                discount = Money.Vnd(DiscountValue);
            }
            else
            {
                discount = orderAmount.ApplyDiscount(DiscountValue);
            }

            if (MaxDiscountAmount != null && discount.Amount > MaxDiscountAmount.Amount)
            {
                discount = MaxDiscountAmount;
            }

            return discount;
        }

        public void IncrementUsage()
        {
            if (UsedCount >= MaxUsage)
                throw new BusinessRuleViolationException("CouponMaxUsage", "Coupon đã hết lượt sử dụng");

            UsedCount++;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void SetConstraints(Money? minOrder, Money? maxDiscount)
        {
            MinOrderAmount = minOrder;
            MaxDiscountAmount = maxDiscount;
        }
    }
}
