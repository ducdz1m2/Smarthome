using Application.Interfaces.Repositories;
using Domain.Entities.Promotions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly AppDbContext _context;

        public CouponRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Coupon?> GetByIdAsync(int id)
        {
            return await _context.Coupons.FindAsync(id);
        }

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            return await _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == code.ToUpper());
        }

        public async Task<List<Coupon>> GetAllAsync()
        {
            return await _context.Coupons
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Coupon>> GetActiveAsync()
        {
            return await _context.Coupons
                .AsNoTracking()
                .Where(c => c.IsActive && c.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Coupon coupon)
        {
            await _context.Coupons.AddAsync(coupon);
        }

        public void Update(Coupon coupon)
        {
            _context.Coupons.Update(coupon);
        }

        public void Delete(Coupon coupon)
        {
            _context.Coupons.Remove(coupon);
        }

        public async Task<bool> ExistsAsync(string code, int? excludeId = null)
        {
            var query = _context.Coupons.Where(c => c.Code == code.ToUpper());
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Coupons.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
