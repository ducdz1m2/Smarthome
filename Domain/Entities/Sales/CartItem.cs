namespace Domain.Entities.Sales;

using Domain.Abstractions;
using Domain.Exceptions;

/// <summary>
/// CartItem entity - represents an item in a user's shopping cart.
/// </summary>
public class CartItem : Entity
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
                throw new ValidationException(nameof(quantity), "Số lượng phải lớn hơn 0");

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
                throw new ValidationException(nameof(newQuantity), "Số lượng phải lớn hơn 0");

            Quantity = newQuantity;
        }
    }
