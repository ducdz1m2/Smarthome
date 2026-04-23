using Domain.Entities.Inventory;
using Domain.Enums;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Warehouse aggregate.
/// </summary>
public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Warehouse?> GetByIdWithStockAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warehouse>> GetActiveAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for ProductWarehouse (stock) entity.
/// </summary>
public interface IProductWarehouseRepository : IRepository<ProductWarehouse>
{
    Task<ProductWarehouse?> GetByProductAndWarehouseAsync(int productId, int warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductWarehouse>> GetByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductWarehouse>> GetByWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductWarehouse>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default);
    Task<int> GetTotalStockAsync(int productId, CancellationToken cancellationToken = default);
    Task<bool> ReserveStockAsync(int productId, int warehouseId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> ReleaseStockAsync(int productId, int warehouseId, int quantity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for StockEntry aggregate.
/// </summary>
public interface IStockEntryRepository : IRepository<StockEntry>
{
    Task<StockEntry?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockEntry>> GetBySupplierAsync(int supplierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockEntry>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockEntry>> GetPendingAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Supplier aggregate.
/// </summary>
public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetByIdWithStockEntriesAsync(int id, CancellationToken cancellationToken = default);
    Task<Supplier?> GetByTaxCodeAsync(string taxCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Supplier>> GetActiveAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for WarehouseTransfer aggregate.
/// </summary>
public interface IWarehouseTransferRepository : IRepository<WarehouseTransfer>
{
    Task<WarehouseTransfer?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarehouseTransfer>> GetByFromWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarehouseTransfer>> GetByToWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarehouseTransfer>> GetByStatusAsync(WarehouseTransferStatus status, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for ProductReservation entity.
/// </summary>
public interface IProductReservationRepository : IRepository<ProductReservation>
{
    Task<IReadOnlyList<ProductReservation>> GetByProductAndWarehouseAsync(int productId, int warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductReservation>> GetByOrderAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductReservation>> GetExpiredAsync(DateTime before, CancellationToken cancellationToken = default);
    Task<int> GetReservedQuantityAsync(int productId, int warehouseId, CancellationToken cancellationToken = default);
}
