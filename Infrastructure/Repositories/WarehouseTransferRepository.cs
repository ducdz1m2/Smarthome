using Application.Interfaces.Repositories;
using Domain.Entities.Inventory;
using Domain.Enums;
using Domain.Events;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WarehouseTransferRepository : Domain.Repositories.IWarehouseTransferRepository
    {
        private readonly AppDbContext _context;
        private readonly IDomainEventDispatcher _eventDispatcher;

        public WarehouseTransferRepository(AppDbContext context, IDomainEventDispatcher eventDispatcher)
        {
            _context = context;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<WarehouseTransfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WarehouseTransfers.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IReadOnlyList<WarehouseTransfer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.WarehouseTransfers
                .AsNoTracking()
                .Include(wt => wt.FromWarehouse)
                .Include(wt => wt.ToWarehouse)
                .OrderByDescending(wt => wt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WarehouseTransfers.AnyAsync(wt => wt.Id == id, cancellationToken);
        }

        public void Add(WarehouseTransfer transfer)
        {
            _context.WarehouseTransfers.Add(transfer);
        }

        public void Update(WarehouseTransfer transfer)
        {
            _context.WarehouseTransfers.Update(transfer);
        }

        public void Delete(WarehouseTransfer transfer)
        {
            _context.WarehouseTransfers.Remove(transfer);
        }

        public async Task<WarehouseTransfer?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WarehouseTransfers
                .Include(wt => wt.FromWarehouse)
                .Include(wt => wt.ToWarehouse)
                .FirstOrDefaultAsync(wt => wt.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<WarehouseTransfer>> GetByFromWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default)
        {
            return await _context.WarehouseTransfers
                .AsNoTracking()
                .Include(wt => wt.ToWarehouse)
                .Where(wt => wt.FromWarehouseId == warehouseId)
                .OrderByDescending(wt => wt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<WarehouseTransfer>> GetByToWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default)
        {
            return await _context.WarehouseTransfers
                .AsNoTracking()
                .Include(wt => wt.FromWarehouse)
                .Where(wt => wt.ToWarehouseId == warehouseId)
                .OrderByDescending(wt => wt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<WarehouseTransfer>> GetByStatusAsync(WarehouseTransferStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.WarehouseTransfers
                .AsNoTracking()
                .Include(wt => wt.FromWarehouse)
                .Include(wt => wt.ToWarehouse)
                .Where(wt => wt.Status == status)
                .OrderByDescending(wt => wt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
