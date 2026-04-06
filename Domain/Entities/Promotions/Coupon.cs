namespace Domain.Entities.Promotions
{
    using Domain.Entities.Common;
    using Domain.Enums;
    using Domain.Exceptions;

    public class Coupon : BaseEntity
    {
        public string Code { get; private set; } = string.Empty;
        public DiscountType DiscountType { get; private set; }
        public decimal DiscountValue { get; private set; }
        public decimal? MinOrderAmount { get; private set; }
        public decimal? MaxDiscountAmount { get; private set; }
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

        public bool IsValid(decimal orderAmount)
        {
            if (!IsActive) return false;
            if (DateTime.UtcNow > ExpiryDate) return false;
            if (UsedCount >= MaxUsage) return false;
            if (MinOrderAmount.HasValue && orderAmount < MinOrderAmount.Value) return false;

            return true;
        }

        public decimal CalculateDiscount(decimal orderAmount)
        {
            if (!IsValid(orderAmount))
                throw new CouponExpiredException(Code, ExpiryDate);

            decimal discount;

            if (DiscountType == DiscountType.FixedAmount)
            {
                discount = DiscountValue;
            }
            else
            {
                discount = orderAmount * DiscountValue / 100;
            }

            if (MaxDiscountAmount.HasValue && discount > MaxDiscountAmount.Value)
            {
                discount = MaxDiscountAmount.Value;
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

        public void SetConstraints(decimal? minOrder, decimal? maxDiscount)
        {
            MinOrderAmount = minOrder;
            MaxDiscountAmount = maxDiscount;
        }
    }
}
