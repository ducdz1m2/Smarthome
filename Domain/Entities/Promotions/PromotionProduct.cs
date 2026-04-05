namespace Domain.Entities.Promotions
{
    using Domain.Entities.Common;
    using Domain.Exceptions;
    using Domain.ValueObjects;

    public class PromotionProduct : BaseEntity
    {
        public int PromotionId { get; private set; }
        public int ProductId { get; private set; }
        public Percentage? CustomDiscountPercent { get; private set; }

        public virtual Promotion Promotion { get; private set; } = null!;

        private PromotionProduct() { }

        public static PromotionProduct Create(int promotionId, int productId, Percentage? customDiscount = null)
        {
            if (promotionId <= 0)
                throw new DomainException("PromotionId không hợp lệ");

            if (productId <= 0)
                throw new DomainException("ProductId không hợp lệ");

            return new PromotionProduct
            {
                PromotionId = promotionId,
                ProductId = productId,
                CustomDiscountPercent = customDiscount
            };
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
}
