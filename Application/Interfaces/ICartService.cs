using Application.DTOs;

namespace Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int userId);
        Task AddToCartAsync(AddToCartRequest request);
        Task UpdateCartItemAsync(int cartItemId, UpdateCartItemRequest request);
        Task RemoveFromCartAsync(int cartItemId);
        Task ClearCartAsync(int userId);
        Task<int> CheckoutAsync(int userId, int shippingAddressId, string? couponCode);
    }
}
