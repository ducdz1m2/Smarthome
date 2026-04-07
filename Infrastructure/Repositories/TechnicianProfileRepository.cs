using Application.Interfaces.Repositories;
using Domain.Entities.Installation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TechnicianProfileRepository : ITechnicianProfileRepository
    {
        private readonly AppDbContext _context;

        public TechnicianProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TechnicianProfile?> GetByIdAsync(int id)
        {
            return await _context.TechnicianProfiles.FindAsync(id);
        }

        public async Task<TechnicianProfile?> GetByUserIdAsync(int userId)
        {
            return await _context.TechnicianProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<TechnicianProfile?> GetByIdWithSlotsAsync(int id)
        {
            return await _context.TechnicianProfiles
                .AsNoTracking()
                .Include(t => t.Slots)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TechnicianProfile>> GetAllAsync()
        {
            return await _context.TechnicianProfiles
                .AsNoTracking()
                .OrderByDescending(t => t.Id)
                .ToListAsync();
        }

        public async Task<List<TechnicianProfile>> GetAvailableAsync()
        {
            return await _context.TechnicianProfiles
                .AsNoTracking()
                .Where(t => t.IsAvailable)
                .OrderByDescending(t => t.Rating)
                .ToListAsync();
        }

        public async Task<List<TechnicianProfile>> GetByDistrictAsync(string district)
        {
            return await _context.TechnicianProfiles
                .AsNoTracking()
                .Where(t => t.Districts.Contains(district))
                .OrderByDescending(t => t.Rating)
                .ToListAsync();
        }

        public async Task<(List<TechnicianProfile> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, bool? isAvailable = null, string? search = null)
        {
            var query = _context.TechnicianProfiles.AsNoTracking().AsQueryable();

            if (isAvailable.HasValue)
                query = query.Where(t => t.IsAvailable == isAvailable.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TechnicianProfiles.AnyAsync(t => t.Id == id);
        }

        public async Task<bool> ExistsByUserIdAsync(int userId)
        {
            return await _context.TechnicianProfiles.AnyAsync(t => t.UserId == userId);
        }

        public async Task<bool> ExistsByEmployeeCodeAsync(string employeeCode)
        {
            return await _context.TechnicianProfiles.AnyAsync(t => t.EmployeeCode == employeeCode.ToUpper());
        }

        public async Task AddAsync(TechnicianProfile technician)
        {
            await _context.TechnicianProfiles.AddAsync(technician);
        }

        public void Update(TechnicianProfile technician)
        {
            _context.TechnicianProfiles.Update(technician);
        }

        public void Delete(TechnicianProfile technician)
        {
            _context.TechnicianProfiles.Remove(technician);
        }

        public async Task<int> CountAsync()
        {
            return await _context.TechnicianProfiles.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
