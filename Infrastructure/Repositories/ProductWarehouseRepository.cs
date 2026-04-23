using Application.Interfaces.Repositories;
using Domain.Abstractions;
using Domain.Entities.Inventory;
using Domain.Events;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductWarehouseRepository : IProductWarehouseRepository
    {
        private readonly AppDbContext _context;
        private readonly IDomainEventDispatcher _eventDispatcher;

        public ProductWarehouseRepository(AppDbContext context, IDomainEventDispatcher eventDispatcher)
        {
            _context = context;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<ProductWarehouse?> GetByIdAsync(int id)
        {
            return await _context.ProductWarehouses.FindAsync(id);
        }

        public async Task<ProductWarehouse?> GetByProductAndWarehouseAsync(int productId, int warehouseId)
        {
            return await _context.ProductWarehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(pw => pw.ProductId == productId && pw.WarehouseId == warehouseId);
        }

        public async Task<ProductWarehouse?> GetByProductVariantAndWarehouseAsync(int productId, int? variantId, int warehouseId)
        {
            var query = _context.ProductWarehouses
                .AsNoTracking()
                .Where(pw => pw.ProductId == productId && pw.WarehouseId == warehouseId);
            
            if (variantId.HasValue)
            {
                query = query.Where(pw => pw.VariantId == variantId.Value);
            }
            
            return await query.FirstOrDefaultAsync();
        }

        public async Task<ProductWarehouse?> GetByProductVariantAndWarehouseForUpdateAsync(int productId, int? variantId, int warehouseId)
        {
            var query = _context.ProductWarehouses
                .Where(pw => pw.ProductId == productId && pw.WarehouseId == warehouseId);
            
            if (variantId.HasValue)
            {
                query = query.Where(pw => pw.VariantId == variantId.Value);
            }
            
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<ProductWarehouse>> GetByProductAsync(int productId)
        {
            return await _context.ProductWarehouses
                .AsNoTracking()
                .Where(pw => pw.ProductId == productId)
                .Include(pw => pw.Warehouse)
                .ToListAsync();
        }

        public async Task<List<ProductWarehouse>> GetByWarehouseAsync(int warehouseId)
        {
            return await _context.ProductWarehouses
                .AsNoTracking()
                .Where(pw => pw.WarehouseId == warehouseId)
                .ToListAsync();
        }

        public async Task<List<ProductWarehouse>> GetAllAsync()
        {
            return await _context.ProductWarehouses
                .AsNoTracking()
                .Include(pw => pw.Warehouse)
                .ToListAsync();
        }

        public async Task<List<ProductWarehouse>> GetByProductsAsync(List<int> productIds)
        {
            return await _context.ProductWarehouses
                .AsNoTracking()
                .Where(pw => productIds.Contains(pw.ProductId))
                .Include(pw => pw.Warehouse)
                .ToListAsync();
        }

        public async Task<List<ProductWarehouse>> GetAvailableWarehousesForProductAsync(int productId)
        {
            return await _context.ProductWarehouses
                .AsNoTracking()
                .Where(pw => pw.ProductId == productId && pw.Quantity > pw.ReservedQuantity)
                .Include(pw => pw.Warehouse)
                .ToListAsync();
        }

        public async Task<List<ProductWarehouse>> GetAvailableWarehousesForProductVariantAsync(int productId, int? variantId)
        {
            var query = _context.ProductWarehouses
                .AsNoTracking()
                .Where(pw => pw.ProductId == productId && pw.Quantity > pw.ReservedQuantity);

            if (variantId.HasValue)
            {
                query = query.Where(pw => pw.VariantId == variantId.Value);
            }
            // If variantId is null, return all warehouses for this product (any variant)

            return await query
                .Include(pw => pw.Warehouse)
                .ToListAsync();
        }

        public async Task AddAsync(ProductWarehouse productWarehouse)
        {
            await _context.ProductWarehouses.AddAsync(productWarehouse);
        }

        public void Update(ProductWarehouse productWarehouse)
        {
            var existing = _context.ProductWarehouses.Local.FirstOrDefault(e => e.Id == productWarehouse.Id);
            if (existing != null)
            {
                _context.Entry(existing).State = EntityState.Detached;
            }
            _context.ProductWarehouses.Update(productWarehouse);
        }

        public void Delete(ProductWarehouse productWarehouse)
        {
            _context.ProductWarehouses.Remove(productWarehouse);
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
    }
}
