using Domain.Entities.Sales;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Services;

/// <summary>
/// Domain service for order processing and workflow management.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Validate if an order can be confirmed.
    /// </summary>
    Task<OrderValidationResult> ValidateOrderForConfirmationAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process order confirmation with stock reservation.
    /// </summary>
    Task<Order> ConfirmOrderAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel an order and release resources.
    /// </summary>
    Task<Order> CancelOrderAsync(
        int orderId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark an order as shipped.
    /// </summary>
    Task<Order> MarkOrderShippedAsync(
        int orderId,
        string trackingNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark an order as delivered.
    /// </summary>
    Task<Order> MarkOrderDeliveredAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete an order.
    /// </summary>
    Task<Order> CompleteOrderAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate shipping fee for an order.
    /// </summary>
    Task<Money> CalculateShippingFeeAsync(
        int orderId,
        ShippingMethod method,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order statistics for a user.
    /// </summary>
    Task<OrderStatistics> GetOrderStatisticsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if all items in an order are fulfilled.
    /// </summary>
    Task<bool> AreAllItemsFulfilledAsync(
        int orderId,
        CancellationToken cancellationToken = default);
}

public record OrderValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = new List<string>();
    public IReadOnlyList<string> Warnings { get; init; } = new List<string>();
    public bool CanProceed => IsValid && !Errors.Any();
}

public record OrderStatistics
{
    public int UserId { get; init; }
    public int TotalOrders { get; init; }
    public int PendingOrders { get; init; }
    public int CompletedOrders { get; init; }
    public int CancelledOrders { get; init; }
    public Money TotalSpent { get; init; } = Money.Zero();
    public Money AverageOrderValue { get; init; } = Money.Zero();
    public DateTime? FirstOrderDate { get; init; }
    public DateTime? LastOrderDate { get; init; }
}
