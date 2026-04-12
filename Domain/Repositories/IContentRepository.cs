using Domain.Entities.Catalog;
using Domain.Entities.Content;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Banner entity.
/// </summary>
public interface IBannerRepository : IRepository<Banner>
{
    Task<IReadOnlyList<Banner>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Banner>> GetActiveByPositionAsync(string position, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Banner>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for ProductComment entity.
/// </summary>
public interface IProductCommentRepository : IRepository<ProductComment>
{
    Task<IReadOnlyList<ProductComment>> GetByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductComment>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductComment>> GetPendingApprovalAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductComment>> GetApprovedByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ProductComment> Items, int TotalCount)> GetPagedByProductAsync(
        int productId,
        int page,
        int pageSize,
        bool approvedOnly = true,
        CancellationToken cancellationToken = default);
    Task<double> GetAverageRatingAsync(int productId, CancellationToken cancellationToken = default);
    Task<int> CountByProductAsync(int productId, CancellationToken cancellationToken = default);
}
