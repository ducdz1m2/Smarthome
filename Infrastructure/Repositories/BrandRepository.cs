using Application.Interfaces.Repositories;
using Domain.Entities.Catalog;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly AppDbContext _context;

        public BrandRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Brand?> GetByIdAsync(int id)
        {
            return await _context.Brands.FindAsync(id);
        }

        public async Task<Brand?> GetByIdWithProductsAsync(int id)
        {
            return await _context.Brands
                .AsNoTracking()
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Brand>> GetAllAsync()
        {
            return await _context.Brands
                .AsNoTracking()
                .Include(b => b.Products)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<List<Brand>> GetActiveAsync()
        {
            return await _context.Brands
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Brand brand)
        {
            await _context.Brands.AddAsync(brand);
        }

        public void Update(Brand brand)
        {
            _context.Brands.Update(brand);
        }

        public void Delete(Brand brand)
        {
            _context.Brands.Remove(brand);
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Brands.Where(b => b.Name == name);
            if (excludeId.HasValue)
                query = query.Where(b => b.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Brands.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
