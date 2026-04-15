using Application.Interfaces.Repositories;
using Domain.Entities.Installation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TechnicianRatingRepository : ITechnicianRatingRepository
    {
        private readonly AppDbContext _context;

        public TechnicianRatingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TechnicianRating?> GetByIdAsync(int id)
        {
            return await _context.TechnicianRatings
                .AsNoTracking()
                .Include(r => r.Technician)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<TechnicianRating>> GetAllAsync()
        {
            return await _context.TechnicianRatings
                .AsNoTracking()
                .Include(r => r.Technician)
                .Include(r => r.Booking)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TechnicianRating>> GetByTechnicianAsync(int technicianId)
        {
            return await _context.TechnicianRatings
                .AsNoTracking()
                .Include(r => r.Technician)
                .Include(r => r.Booking)
                .Where(r => r.TechnicianId == technicianId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TechnicianRating>> GetByUserAsync(int userId)
        {
            return await _context.TechnicianRatings
                .AsNoTracking()
                .Include(r => r.Technician)
                .Include(r => r.Booking)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TechnicianRating>> GetByBookingAsync(int bookingId)
        {
            return await _context.TechnicianRatings
                .AsNoTracking()
                .Include(r => r.Technician)
                .Include(r => r.Booking)
                .Where(r => r.BookingId == bookingId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<TechnicianRating?> GetByTechnicianAndBookingAsync(int technicianId, int bookingId)
        {
            return await _context.TechnicianRatings
                .AsNoTracking()
                .Include(r => r.Technician)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.TechnicianId == technicianId && r.BookingId == bookingId);
        }

        public async Task<List<TechnicianRating>> GetPendingApprovalAsync()
        {
            return await _context.TechnicianRatings
                .AsNoTracking()
                .Include(r => r.Technician)
                .Include(r => r.Booking)
                .Where(r => !r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.TechnicianRatings.CountAsync();
        }

        public async Task<int> CountPendingAsync()
        {
            return await _context.TechnicianRatings
                .CountAsync(r => !r.IsApproved);
        }

        public async Task AddAsync(TechnicianRating rating)
        {
            await _context.TechnicianRatings.AddAsync(rating);
        }

        public void Update(TechnicianRating rating)
        {
            _context.TechnicianRatings.Update(rating);
        }

        public void Delete(TechnicianRating rating)
        {
            _context.TechnicianRatings.Remove(rating);
        }

        public async Task DeleteByIdAsync(int id)
        {
            var rating = await _context.TechnicianRatings.FindAsync(id);
            if (rating != null)
            {
                _context.TechnicianRatings.Remove(rating);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
