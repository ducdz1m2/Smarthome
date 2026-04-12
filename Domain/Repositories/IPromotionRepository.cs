using Domain.Entities.Promotions;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Promotion aggregate.
/// </summary>
public interface IPromotionRepository : IRepository<Promotion>
{
    Task<Promotion?> GetByIdWithProductsAsync(int id, CancellationToken cancellationToken = default);
    Task<Promotion?> GetActiveByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Promotion>> GetActiveAsync(DateTime? asOfDate = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Promotion>> GetByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Promotion>> GetUpcomingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Promotion>> GetExpiredAsync(CancellationToken cancellationToken = default);
    Task<bool> IsProductInPromotionAsync(int productId, int promotionId, CancellationToken cancellationToken = default);
    Task AddProductToPromotionAsync(int productId, int promotionId, CancellationToken cancellationToken = default);
    Task RemoveProductFromPromotionAsync(int productId, int promotionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Coupon aggregate.
/// </summary>
public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Coupon?> GetActiveByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Coupon>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Coupon>> GetByPromotionAsync(int promotionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Coupon>> GetExpiredAsync(CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasBeenUsedAsync(int couponId, int userId, CancellationToken cancellationToken = default);
    Task IncrementUsageAsync(int couponId, CancellationToken cancellationToken = default);
}
