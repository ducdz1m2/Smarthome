using Application.Interfaces.Repositories;
using Domain.Entities.Promotions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly AppDbContext _context;

        public PromotionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Promotion?> GetByIdAsync(int id)
        {
            return await _context.Promotions.FindAsync(id);
        }

        public async Task<Promotion?> GetByIdWithProductsAsync(int id)
        {
            return await _context.Promotions
                .Include(p => p.PromotionProducts)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Promotion>> GetAllAsync()
        {
            return await _context.Promotions
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Promotion>> GetActiveAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Promotions
                .AsNoTracking()
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .OrderByDescending(p => p.Priority)
                .ThenBy(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<List<Promotion>> GetActiveForProductAsync(int productId)
        {
            var now = DateTime.UtcNow;
            return await _context.Promotions
                .AsNoTracking()
                .Include(p => p.PromotionProducts)
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .Where(p => !p.PromotionProducts.Any() || p.PromotionProducts.Any(pp => pp.ProductId == productId))
                .OrderByDescending(p => p.Priority)
                .ThenBy(p => p.StartDate)
                .ToListAsync();
        }

        public async Task AddAsync(Promotion promotion)
        {
            await _context.Promotions.AddAsync(promotion);
        }

        public void Update(Promotion promotion)
        {
            _context.Promotions.Update(promotion);
        }

        public void Delete(Promotion promotion)
        {
            _context.Promotions.Remove(promotion);
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Promotions.Where(p => p.Name == name);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Promotions.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
