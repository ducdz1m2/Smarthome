using Application.Interfaces.Repositories;
using Domain.Entities.Inventory;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly AppDbContext _context;

        public WarehouseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _context.Warehouses.FindAsync(id);
        }

        public async Task<List<Warehouse>> GetAllAsync()
        {
            return await _context.Warehouses
                .AsNoTracking()
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<List<Warehouse>> GetActiveAsync()
        {
            return await _context.Warehouses
                .AsNoTracking()
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Warehouse warehouse)
        {
            await _context.Warehouses.AddAsync(warehouse);
        }

        public void Update(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
        }

        public void Delete(Warehouse warehouse)
        {
            _context.Warehouses.Remove(warehouse);
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Warehouses.Where(w => w.Name == name);
            if (excludeId.HasValue)
                query = query.Where(w => w.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _context.Warehouses.Where(w => w.Code == code);
            if (excludeId.HasValue)
                query = query.Where(w => w.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Warehouses.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
