using Domain.Abstractions;
using Domain.Entities.Catalog;
using Domain.Events;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductRepository : Application.Interfaces.Repositories.IProductRepository, IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly IDomainEventDispatcher _eventDispatcher;

        public ProductRepository(AppDbContext context, IDomainEventDispatcher eventDispatcher)
        {
            _context = context;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Supplier)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetByIdForUpdateAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Supplier)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetBySkuAsync(string sku)
        {
            var skuValue = Domain.ValueObjects.Sku.Create(sku.ToUpper());
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Sku == skuValue);
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }

        public async Task<List<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public async Task<List<Product>> GetByBrandAsync(int brandId)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.BrandId == brandId)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? search = null, int? categoryId = null, int? brandId = null, bool? isActive = null)
        {
            var query = _context.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || EF.Functions.Like(p.Sku.ToString(), $"%{search}%"));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsAsync(string sku, int? excludeId = null)
        {
            var skuValue = Domain.ValueObjects.Sku.Create(sku.ToUpper());
            var query = _context.Products.Where(p => p.Sku == skuValue);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
        }

        public void Delete(Product product)
        {
            _context.Products.Remove(product);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Products.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            // Get all aggregate roots with domain events before saving
            var aggregatesWithEvents = _context.ChangeTracker.Entries<AggregateRoot>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            // Collect all domain events (cast from INotification to DomainEvent)
            var domainEvents = aggregatesWithEvents
                .SelectMany(a => a.DomainEvents)
                .OfType<DomainEvent>()
                .ToList();

            // Clear domain events from aggregates
            foreach (var aggregate in aggregatesWithEvents)
            {
                aggregate.ClearDomainEvents();
            }

            // Save changes to database
            await _context.SaveChangesAsync();

            // Dispatch domain events after saving
            foreach (var domainEvent in domainEvents)
            {
                await _eventDispatcher.DispatchAsync(domainEvent);
            }
        }

        public async Task<List<Product>> SearchAsync(string keyword, int? categoryId)
        {
            var query = _context.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Name.Contains(keyword) || EF.Functions.Like(p.Sku.ToString(), $"%{keyword}%"));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            return await query
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .ToListAsync();
        }
    }
}
