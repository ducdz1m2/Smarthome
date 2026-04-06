using Domain.Entities.Promotions;

namespace Application.Interfaces.Repositories
{
    public interface ICouponRepository
    {
        Task<Coupon?> GetByIdAsync(int id);
        Task<Coupon?> GetByCodeAsync(string code);
        Task<List<Coupon>> GetAllAsync();
        Task<List<Coupon>> GetActiveAsync();
        Task AddAsync(Coupon coupon);
        void Update(Coupon coupon);
        void Delete(Coupon coupon);
        Task<bool> ExistsAsync(string code, int? excludeId = null);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
