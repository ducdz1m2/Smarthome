using Application.Interfaces.Repositories;
using Domain.Entities.Sales;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WarrantyClaimRepository : IWarrantyClaimRepository
    {
        private readonly AppDbContext _context;

        public WarrantyClaimRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WarrantyClaim?> GetByIdAsync(int id)
        {
            return await _context.WarrantyClaims.FindAsync(id);
        }

        public async Task<List<WarrantyClaim>> GetAllAsync()
        {
            return await _context.WarrantyClaims
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<WarrantyClaim>> GetByWarrantyIdAsync(int warrantyId)
        {
            return await _context.WarrantyClaims
                .AsNoTracking()
                .Where(c => c.WarrantyId == warrantyId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<WarrantyClaim>> GetByStatusAsync(WarrantyClaimStatus status)
        {
            return await _context.WarrantyClaims
                .AsNoTracking()
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<WarrantyClaim>> GetPendingClaimsAsync()
        {
            return await _context.WarrantyClaims
                .AsNoTracking()
                .Where(c => c.Status == WarrantyClaimStatus.Pending)
                .OrderBy(c => c.ClaimDate)
                .ToListAsync();
        }

        public async Task AddAsync(WarrantyClaim claim)
        {
            await _context.WarrantyClaims.AddAsync(claim);
        }

        public void Update(WarrantyClaim claim)
        {
            _context.WarrantyClaims.Update(claim);
        }

        public void Delete(WarrantyClaim claim)
        {
            _context.WarrantyClaims.Remove(claim);
        }

        public async Task<int> CountAsync()
        {
            return await _context.WarrantyClaims.CountAsync();
        }

        public async Task<int> CountByStatusAsync(WarrantyClaimStatus status)
        {
            return await _context.WarrantyClaims.CountAsync(c => c.Status == status);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
