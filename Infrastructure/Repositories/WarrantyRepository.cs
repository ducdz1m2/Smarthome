using Application.Interfaces.Repositories;
using Domain.Entities.Sales;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WarrantyRepository : IWarrantyRepository
    {
        private readonly AppDbContext _context;

        public WarrantyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Warranty?> GetByIdAsync(int id)
        {
            return await _context.Warranties.FindAsync(id);
        }

        public async Task<Warranty?> GetByIdWithClaimsAsync(int id)
        {
            return await _context.Warranties
                .Include(w => w.Claims)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<List<Warranty>> GetAllAsync()
        {
            return await _context.Warranties
                .AsNoTracking()
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Warranty>> GetByProductIdAsync(int productId)
        {
            return await _context.Warranties
                .AsNoTracking()
                .Where(w => w.ProductId == productId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Warranty>> GetByOrderItemIdAsync(int orderItemId)
        {
            return await _context.Warranties
                .AsNoTracking()
                .Where(w => w.OrderItemId == orderItemId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Warranty>> GetActiveWarrantiesAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Warranties
                .AsNoTracking()
                .Where(w => w.Status == WarrantyStatus.Active && w.EndDate > now)
                .OrderByDescending(w => w.EndDate)
                .ToListAsync();
        }

        public async Task AddAsync(Warranty warranty)
        {
            await _context.Warranties.AddAsync(warranty);
        }

        public void Update(Warranty warranty)
        {
            _context.Warranties.Update(warranty);
        }

        public void Delete(Warranty warranty)
        {
            _context.Warranties.Remove(warranty);
        }

        public async Task<bool> ExistsAsync(int orderItemId)
        {
            return await _context.Warranties.AnyAsync(w => w.OrderItemId == orderItemId);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Warranties.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
