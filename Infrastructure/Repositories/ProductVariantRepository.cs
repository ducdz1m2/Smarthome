using Application.Interfaces.Repositories;
using Domain.Entities.Catalog;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly AppDbContext _context;

        public ProductVariantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductVariant?> GetByIdAsync(int id)
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<ProductVariant?> GetBySkuAsync(string sku)
        {
            var skuValue = Domain.ValueObjects.Sku.Create(sku.ToUpper());
            return await _context.ProductVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Sku == skuValue);
        }

        public async Task<List<ProductVariant>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .AsNoTracking()
                .Where(v => v.ProductId == productId)
                .OrderBy(v => v.Id)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string sku, int? excludeId = null)
        {
            var skuValue = Domain.ValueObjects.Sku.Create(sku.ToUpper());
            var query = _context.ProductVariants.Where(v => v.Sku == skuValue);
            if (excludeId.HasValue)
                query = query.Where(v => v.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task AddAsync(ProductVariant variant)
        {
            await _context.ProductVariants.AddAsync(variant);
        }

        public void Update(ProductVariant variant)
        {
            _context.ProductVariants.Update(variant);
        }

        public void Delete(ProductVariant variant)
        {
            _context.ProductVariants.Remove(variant);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
