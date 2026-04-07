using Application.Interfaces.Repositories;
using Domain.Entities.Inventory;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class StockEntryRepository : IStockEntryRepository
    {
        private readonly AppDbContext _context;

        public StockEntryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StockEntry?> GetByIdAsync(int id)
        {
            return await _context.StockEntries.FindAsync(id);
        }

        public async Task<StockEntry?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.StockEntries
                .AsNoTracking()
                .Include(se => se.Supplier)
                .Include(se => se.Warehouse)
                .Include(se => se.Details)
                .ThenInclude(d => d.Product)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(se => se.Id == id);
        }

        public async Task<List<StockEntry>> GetAllAsync()
        {
            return await _context.StockEntries
                .AsNoTracking()
                .Include(se => se.Supplier)
                .Include(se => se.Warehouse)
                .OrderByDescending(se => se.EntryDate)
                .ToListAsync();
        }

        public async Task<List<StockEntry>> GetByWarehouseAsync(int warehouseId)
        {
            return await _context.StockEntries
                .AsNoTracking()
                .Where(se => se.WarehouseId == warehouseId)
                .Include(se => se.Supplier)
                .OrderByDescending(se => se.EntryDate)
                .ToListAsync();
        }

        public async Task<List<StockEntry>> GetBySupplierAsync(int supplierId)
        {
            return await _context.StockEntries
                .AsNoTracking()
                .Where(se => se.SupplierId == supplierId)
                .Include(se => se.Warehouse)
                .OrderByDescending(se => se.EntryDate)
                .ToListAsync();
        }

        public async Task<List<StockEntry>> GetFilteredAsync(int? warehouseId, int? supplierId, bool? isCompleted)
        {
            var query = _context.StockEntries.AsNoTracking().AsQueryable();

            if (warehouseId.HasValue)
                query = query.Where(se => se.WarehouseId == warehouseId.Value);

            if (supplierId.HasValue)
                query = query.Where(se => se.SupplierId == supplierId.Value);

            if (isCompleted.HasValue)
                query = query.Where(se => se.IsCompleted == isCompleted.Value);

            return await query
                .Include(se => se.Supplier)
                .Include(se => se.Warehouse)
                .Include(se => se.Details)
                .OrderByDescending(se => se.EntryDate)
                .ToListAsync();
        }

        public async Task AddAsync(StockEntry stockEntry)
        {
            await _context.StockEntries.AddAsync(stockEntry);
        }

        public void Update(StockEntry stockEntry)
        {
            _context.StockEntries.Update(stockEntry);
        }

        public void Delete(StockEntry stockEntry)
        {
            _context.StockEntries.Remove(stockEntry);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
