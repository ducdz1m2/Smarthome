using Domain.Entities.Inventory;
using Domain.ValueObjects;

namespace Domain.Services;

/// <summary>
/// Domain service for inventory management operations.
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Check if there's sufficient stock for an order.
    /// </summary>
    Task<bool> IsStockAvailableAsync(
        int productId,
        int quantity,
        int? preferredWarehouseId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the best warehouse to fulfill an order from.
    /// </summary>
    Task<Warehouse?> GetBestWarehouseForFulfillmentAsync(
        int productId,
        int quantity,
        string? targetDistrict = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserve stock for an order.
    /// </summary>
    Task<StockReservationResult> ReserveStockAsync(
        int productId,
        int quantity,
        int orderId,
        int? warehouseId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Release reserved stock for an order.
    /// </summary>
    Task<bool> ReleaseStockAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm stock deduction after order completion.
    /// </summary>
    Task<bool> DeductStockAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfer stock between warehouses.
    /// </summary>
    Task<WarehouseTransfer> InitiateTransferAsync(
        int fromWarehouseId,
        int toWarehouseId,
        Dictionary<int, int> productQuantities,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current stock level for a product across all warehouses.
    /// </summary>
    Task<IReadOnlyList<StockLevel>> GetStockLevelsAsync(
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check for low stock alerts.
    /// </summary>
    Task<IReadOnlyList<LowStockAlert>> CheckLowStockAlertsAsync(
        CancellationToken cancellationToken = default);
}

public record StockReservationResult
{
    public bool Success { get; init; }
    public int? ReservationId { get; init; }
    public int WarehouseId { get; init; }
    public string? ErrorMessage { get; init; }
    public int ReservedQuantity { get; init; }

    public static StockReservationResult SuccessResult(int reservationId, int warehouseId, int quantity) =>
        new() { Success = true, ReservationId = reservationId, WarehouseId = warehouseId, ReservedQuantity = quantity };

    public static StockReservationResult FailureResult(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}

public record StockLevel
{
    public int ProductId { get; init; }
    public int WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public int AvailableQuantity { get; init; }
    public int ReservedQuantity { get; init; }
    public int FrozenQuantity { get; init; }
    public int ActualQuantity => AvailableQuantity + ReservedQuantity + FrozenQuantity;
    public Money? CostPrice { get; init; }
}

public record LowStockAlert
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public int WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public int CurrentQuantity { get; init; }
    public int Threshold { get; init; }
    public int Shortage => Threshold - CurrentQuantity;
}
