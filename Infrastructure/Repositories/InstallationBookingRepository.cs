using Application.Interfaces.Repositories;
using Domain.Entities.Installation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class InstallationBookingRepository : IInstallationBookingRepository
    {
        private readonly AppDbContext _context;

        public InstallationBookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InstallationBooking?> GetByIdAsync(int id)
        {
            return await _context.InstallationBookings.FindAsync(id);
        }

        public async Task<InstallationBooking?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.InstallationBookings
                .AsNoTracking()
                .Include(b => b.Order)
                    .ThenInclude(o => o.Items)
                        .ThenInclude(i => i.Product)
                .Include(b => b.Technician)
                .Include(b => b.Slot)
                .Include(b => b.Materials)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<InstallationBooking?> GetByOrderIdAsync(int orderId)
        {
            return await _context.InstallationBookings
                .AsNoTracking()
                .Include(b => b.Order)
                .Include(b => b.Technician)
                .Include(b => b.Slot)
                .Include(b => b.Materials)
                .FirstOrDefaultAsync(b => b.OrderId == orderId);
        }

        public async Task<List<InstallationBooking>> GetAllAsync()
        {
            return await _context.InstallationBookings
                .AsNoTracking()
                .Include(b => b.Order)
                .Include(b => b.Technician)
                .Include(b => b.Slot)
                .OrderByDescending(b => b.Id)
                .ToListAsync();
        }

        public async Task<List<InstallationBooking>> GetByTechnicianIdAsync(int technicianId)
        {
            return await _context.InstallationBookings
                .AsNoTracking()
                .Where(b => b.TechnicianId == technicianId)
                .Include(b => b.Order)
                .Include(b => b.Slot)
                .OrderByDescending(b => b.ScheduledDate)
                .ToListAsync();
        }

        public async Task<List<InstallationBooking>> GetByStatusAsync(string status)
        {
            return await _context.InstallationBookings
                .AsNoTracking()
                .Where(b => b.Status.ToString() == status)
                .Include(b => b.Order)
                .Include(b => b.Technician)
                .Include(b => b.Slot)
                .OrderByDescending(b => b.ScheduledDate)
                .ToListAsync();
        }

        public async Task<(List<InstallationBooking> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, int? technicianId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.InstallationBookings.AsNoTracking().AsQueryable();

            if (technicianId.HasValue)
                query = query.Where(b => b.TechnicianId == technicianId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(b => b.Status.ToString() == status);

            if (fromDate.HasValue)
                query = query.Where(b => b.ScheduledDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(b => b.ScheduledDate <= toDate.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(b => b.Order)
                .Include(b => b.Technician)
                .Include(b => b.Slot)
                .OrderByDescending(b => b.ScheduledDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.InstallationBookings.AnyAsync(b => b.Id == id);
        }

        public async Task<bool> ExistsByOrderIdAsync(int orderId)
        {
            return await _context.InstallationBookings.AnyAsync(b => b.OrderId == orderId);
        }

        public async Task AddAsync(InstallationBooking booking)
        {
            await _context.InstallationBookings.AddAsync(booking);
        }

        public void Update(InstallationBooking booking)
        {
            _context.InstallationBookings.Update(booking);
        }

        public void Delete(InstallationBooking booking)
        {
            _context.InstallationBookings.Remove(booking);
        }

        public async Task<int> CountAsync()
        {
            return await _context.InstallationBookings.CountAsync();
        }

        public async Task<int> CountByStatusAsync(string status)
        {
            return await _context.InstallationBookings.CountAsync(b => b.Status.ToString() == status);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
