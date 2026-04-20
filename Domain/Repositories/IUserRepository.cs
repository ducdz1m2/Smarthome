using Domain.Entities.Content;
using Domain.Entities.Identity;
using Domain.Entities.Sales;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate.
/// Note: ApplicationUser inherits from IdentityUser, not Entity, so this doesn't use IRepository<T>.
/// </summary>
public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByIdWithAddressesAsync(int id, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByIdWithOrdersAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> PhoneExistsAsync(string phone, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationUser>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationUser>> SearchAsync(string keyword, CancellationToken cancellationToken = default);
    Task SaveAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for UserAddress entity.
/// </summary>
public interface IUserAddressRepository : IRepository<UserAddress>
{
    Task<IReadOnlyList<UserAddress>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserAddress?> GetDefaultForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task SetDefaultAddressAsync(int userId, int addressId, CancellationToken cancellationToken = default);
    Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Warranty aggregate.
/// </summary>
public interface IWarrantyRepository : IRepository<Warranty>
{
    Task<Warranty?> GetByIdWithClaimsAsync(int id, CancellationToken cancellationToken = default);
    Task<Warranty?> GetByOrderItemIdAsync(int orderItemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warranty>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warranty>> GetByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warranty>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warranty>> GetExpiringSoonAsync(int days, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warranty>> GetExpiredAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for WarrantyClaim entity.
/// </summary>
public interface IWarrantyClaimRepository : IRepository<WarrantyClaim>
{
    Task<IReadOnlyList<WarrantyClaim>> GetByWarrantyAsync(int warrantyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarrantyClaim>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarrantyClaim>> GetByStatusAsync(Domain.Entities.Sales.WarrantyClaimStatus status, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for ReturnOrder aggregate.
/// </summary>
public interface IReturnOrderRepository : IRepository<ReturnOrder>
{
    Task<ReturnOrder?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<ReturnOrder?> GetByOriginalOrderAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReturnOrder>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReturnOrder>> GetPendingAsync(CancellationToken cancellationToken = default);
}
