namespace Domain.Entities.Sales
{
    using Domain.Entities.Common;
    using Domain.Exceptions;

    public class CartItem : BaseEntity
    {
        public int UserId { get; private set; }
        public int ProductId { get; private set; }
        public int? VariantId { get; private set; }
        public int Quantity { get; private set; }
        public DateTime AddedAt { get; private set; }

        private CartItem() { }

        public static CartItem Create(int userId, int productId, int? variantId, int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Số lượng phải lớn hơn 0");

            return new CartItem
            {
                UserId = userId,
                ProductId = productId,
                VariantId = variantId,
                Quantity = quantity,
                AddedAt = DateTime.UtcNow
            };
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new DomainException("Số lượng phải lớn hơn 0");

            Quantity = newQuantity;
        }
    }
}
