namespace Domain.Repositories;

using Domain.Entities.Inventory;

/// <summary>
/// Repository interface for StockIssue aggregate.
/// </summary>
public interface IStockIssueRepository : IRepository<StockIssue>
{
    Task<StockIssue?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockIssue>> GetByWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockIssue>> GetByBookingAsync(int bookingId, CancellationToken cancellationToken = default);
    void Add(StockIssueDetail entity);
}
