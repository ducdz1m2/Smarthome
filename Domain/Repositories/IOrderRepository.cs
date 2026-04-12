using Domain.Entities.Sales;
using Domain.Enums;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Order aggregate.
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithItemsAndShipmentsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByUserPagedAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        OrderStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersRequiringInstallationAsync(CancellationToken cancellationToken = default);
    Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for OrderItem entity.
/// </summary>
public interface IOrderItemRepository : IRepository<OrderItem>
{
    Task<IReadOnlyList<OrderItem>> GetByOrderAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderItem>> GetByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderItem>> GetPendingInstallationItemsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for CartItem entity.
/// </summary>
public interface ICartItemRepository : IRepository<CartItem>
{
    Task<IReadOnlyList<CartItem>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<CartItem?> GetByUserAndProductAsync(int userId, int productId, int? variantId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(int userId, CancellationToken cancellationToken = default);
}
