namespace Domain.Entities.Promotions;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// Coupon aggregate root - represents a discount coupon code.
/// </summary>
public class Coupon : AggregateRoot
    {
        public string Code { get; private set; } = string.Empty;
        public DiscountType DiscountType { get; private set; }
        public Money DiscountValue { get; private set; } = null!;
        public Money? MinOrderAmount { get; private set; }
        public Money? MaxDiscountAmount { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public int MaxUsage { get; private set; }
        public int UsedCount { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Coupon() { }

        public static Coupon Create(string code, DiscountType type, Money value, DateTime expiry, int maxUsage = 100, Money? minOrderAmount = null, Money? maxDiscountAmount = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ValidationException(nameof(code), "Mã coupon không được trống");

            if (value.IsLessThanOrEqualTo(Money.Zero()))
                throw new ValidationException(nameof(value), "Giá trị giảm giá phải lớn hơn 0");

            if (expiry <= DateTime.UtcNow)
                throw new ValidationException(nameof(expiry), "Ngày hết hạn phải trong tương lai");

            return new Coupon
            {
                Code = code.Trim().ToUpper(),
                DiscountType = type,
                DiscountValue = value,
                MinOrderAmount = minOrderAmount,
                MaxDiscountAmount = maxDiscountAmount,
                ExpiryDate = expiry,
                MaxUsage = maxUsage,
                UsedCount = 0,
                IsActive = true
            };
        }

        // Legacy overload for backward compatibility
        public static Coupon Create(string code, DiscountType type, decimal value, DateTime expiry, int maxUsage = 100)
        {
            return Create(code, type, Money.Vnd(value), expiry, maxUsage);
        }

        public bool IsValid(Money orderAmount)
        {
            if (!IsActive) return false;
            if (DateTime.UtcNow > ExpiryDate) return false;
            if (UsedCount >= MaxUsage) return false;
            if (MinOrderAmount != null && orderAmount.IsLessThan(MinOrderAmount)) return false;

            return true;
        }

        // Legacy overload for backward compatibility
        public bool IsValid(decimal orderAmount)
        {
            return IsValid(Money.Vnd(orderAmount));
        }

        public Money CalculateDiscount(Money orderAmount)
        {
            if (!IsValid(orderAmount))
                throw new InvalidCouponException(Code, "Coupon is not valid for this order");

            Money discount;

            if (DiscountType == DiscountType.FixedAmount)
            {
                discount = DiscountValue;
            }
            else
            {
                discount = orderAmount.ApplyDiscount(DiscountValue.Amount);
            }

            if (MaxDiscountAmount != null && discount.IsGreaterThan(MaxDiscountAmount))
            {
                discount = MaxDiscountAmount;
            }

            return discount;
        }

        // Legacy overload for backward compatibility
        public decimal CalculateDiscount(decimal orderAmount)
        {
            return CalculateDiscount(Money.Vnd(orderAmount)).Amount;
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
