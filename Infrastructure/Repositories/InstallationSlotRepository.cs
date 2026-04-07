using Application.Interfaces.Repositories;
using Domain.Entities.Installation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class InstallationSlotRepository : IInstallationSlotRepository
    {
        private readonly AppDbContext _context;

        public InstallationSlotRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InstallationSlot?> GetByIdAsync(int id)
        {
            return await _context.InstallationSlots.FindAsync(id);
        }

        public async Task<List<InstallationSlot>> GetAllAsync()
        {
            return await _context.InstallationSlots
                .AsNoTracking()
                .Include(s => s.Technician)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
        }

        public async Task<List<InstallationSlot>> GetByTechnicianIdAsync(int technicianId)
        {
            return await _context.InstallationSlots
                .AsNoTracking()
                .Where(s => s.TechnicianId == technicianId)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<InstallationSlot>> GetByTechnicianAndDateAsync(int technicianId, DateTime date)
        {
            return await _context.InstallationSlots
                .AsNoTracking()
                .Where(s => s.TechnicianId == technicianId && s.Date.Date == date.Date)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<InstallationSlot>> GetAvailableSlotsAsync(int technicianId, DateTime date)
        {
            return await _context.InstallationSlots
                .AsNoTracking()
                .Where(s => s.TechnicianId == technicianId && s.Date.Date == date.Date && !s.IsBooked)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<(List<InstallationSlot> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, int? technicianId = null, DateTime? date = null, bool? isBooked = null)
        {
            var query = _context.InstallationSlots.AsNoTracking().AsQueryable();

            if (technicianId.HasValue)
                query = query.Where(s => s.TechnicianId == technicianId.Value);

            if (date.HasValue)
                query = query.Where(s => s.Date.Date == date.Value.Date);

            if (isBooked.HasValue)
                query = query.Where(s => s.IsBooked == isBooked.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(s => s.Technician)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.InstallationSlots.AnyAsync(s => s.Id == id);
        }

        public async Task<bool> HasOverlapAsync(int technicianId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var query = _context.InstallationSlots
                .Where(s => s.TechnicianId == technicianId && s.Date.Date == date.Date);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return await query.AnyAsync(s =>
                (startTime < s.EndTime && endTime > s.StartTime));
        }

        public async Task AddAsync(InstallationSlot slot)
        {
            await _context.InstallationSlots.AddAsync(slot);
        }

        public async Task AddRangeAsync(List<InstallationSlot> slots)
        {
            await _context.InstallationSlots.AddRangeAsync(slots);
        }

        public void Update(InstallationSlot slot)
        {
            _context.InstallationSlots.Update(slot);
        }

        public void Delete(InstallationSlot slot)
        {
            _context.InstallationSlots.Remove(slot);
        }

        public void DeleteRange(List<InstallationSlot> slots)
        {
            _context.InstallationSlots.RemoveRange(slots);
        }

        public async Task<int> CountAsync()
        {
            return await _context.InstallationSlots.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
