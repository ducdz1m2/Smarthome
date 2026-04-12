namespace Domain.Entities.Promotions;

using Domain.Abstractions;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// PromotionProduct entity - links a product to a promotion with optional custom discount.
/// </summary>
public class PromotionProduct : Entity
{
    public int PromotionId { get; private set; }
    public int ProductId { get; private set; }
    public Percentage? CustomDiscountPercent { get; private set; }

        public virtual Promotion Promotion { get; private set; } = null!;

        private PromotionProduct() { }

        public static PromotionProduct Create(int promotionId, int productId, Percentage? customDiscount = null)
        {
            if (promotionId <= 0)
                throw new ValidationException(nameof(promotionId), "PromotionId không hợp lệ");

            if (productId <= 0)
                throw new ValidationException(nameof(productId), "ProductId không hợp lệ");

            return new PromotionProduct
            {
                PromotionId = promotionId,
                ProductId = productId,
                CustomDiscountPercent = customDiscount
            };
        }

        // Legacy overload for backward compatibility
        public static PromotionProduct Create(int promotionId, int productId, decimal? customDiscount = null)
        {
            return Create(promotionId, productId, customDiscount.HasValue ? Percentage.Create(customDiscount.Value) : null);
        }

        public void UpdateCustomDiscount(Percentage? customDiscount)
        {
            CustomDiscountPercent = customDiscount;
        }

        public Percentage GetEffectiveDiscount(Percentage defaultDiscount)
        {
            return CustomDiscountPercent ?? defaultDiscount;
        }
    }
