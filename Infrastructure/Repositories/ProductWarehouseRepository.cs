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
                .FirstOrDefaultAsync(pw => pw.ProductId == productId && pw.WarehouseId == warehouseId);
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

        public async Task AddAsync(ProductWarehouse productWarehouse)
        {
            await _context.ProductWarehouses.AddAsync(productWarehouse);
        }

        public void Update(ProductWarehouse productWarehouse)
        {
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
