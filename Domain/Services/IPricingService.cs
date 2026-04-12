using Domain.Entities.Catalog;
using Domain.Entities.Sales;
using Domain.ValueObjects;

namespace Domain.Services;

/// <summary>
/// Domain service for calculating product prices with promotions and discounts.
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Calculate the final price for a product considering all applicable promotions.
    /// </summary>
    Task<Money> CalculatePriceAsync(
        Product product,
        int quantity,
        string? couponCode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate the subtotal for an order item.
    /// </summary>
    Task<Money> CalculateOrderItemPriceAsync(
        int productId,
        int? variantId,
        int quantity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate the discount amount for an order.
    /// </summary>
    Task<Money> CalculateOrderDiscountAsync(
        Order order,
        string? couponCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active promotions for a product.
    /// </summary>
    Task<IReadOnlyList<Domain.Entities.Promotions.Promotion>> GetActivePromotionsForProductAsync(
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate and apply a coupon to an order.
    /// </summary>
    Task<(bool IsValid, Money DiscountAmount, string? ErrorMessage)> ValidateAndApplyCouponAsync(
        Order order,
        string couponCode,
        CancellationToken cancellationToken = default);
}
