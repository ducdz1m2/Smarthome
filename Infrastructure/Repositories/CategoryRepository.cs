using Application.Interfaces.Repositories;
using Domain.Entities.Catalog;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category?> GetByIdWithChildrenAsync(int id)
        {
            return await _context.Categories
                .AsNoTracking()
                .Include(c => c.Children)
                .Include(c => c.Products)
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Include(c => c.Children)
                .Include(c => c.Products)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
        }

        public async Task<List<Category>> GetActiveAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
        }

        public async Task<List<Category>> GetRootCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.ParentId == null)
                .Include(c => c.Children)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }

        public void Delete(Category category)
        {
            _context.Categories.Remove(category);
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Categories.Where(c => c.Name == name);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Categories.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
