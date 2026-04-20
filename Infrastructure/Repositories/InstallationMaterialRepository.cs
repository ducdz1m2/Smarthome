using Domain.Entities.Installation;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class InstallationMaterialRepository : IInstallationMaterialRepository
    {
        private readonly AppDbContext _context;

        public InstallationMaterialRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<InstallationMaterial>> GetByBookingAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            return await _context.InstallationMaterials
                .Where(m => m.BookingId == bookingId)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalCostByBookingAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            // InstallationMaterial doesn't have UnitPrice, return 0 for now
            // This would need to be implemented by joining with Product table if needed
            var materials = await _context.InstallationMaterials
                .Where(m => m.BookingId == bookingId)
                .ToListAsync(cancellationToken);
            return 0;
        }

        public async Task<InstallationMaterial?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.InstallationMaterials.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IReadOnlyList<InstallationMaterial>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.InstallationMaterials.ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.InstallationMaterials.AnyAsync(m => m.Id == id, cancellationToken);
        }

        public async Task AddAsync(InstallationMaterial entity, CancellationToken cancellationToken = default)
        {
            await _context.InstallationMaterials.AddAsync(entity, cancellationToken);
        }

        public void Add(InstallationMaterial entity)
        {
            _context.InstallationMaterials.Add(entity);
        }

        public void Update(InstallationMaterial entity)
        {
            _context.InstallationMaterials.Update(entity);
        }

        public void Delete(InstallationMaterial entity)
        {
            _context.InstallationMaterials.Remove(entity);
        }
    }
}
