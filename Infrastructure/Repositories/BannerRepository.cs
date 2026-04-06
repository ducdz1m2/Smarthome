using Application.Interfaces.Repositories;
using Domain.Entities.Content;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BannerRepository : IBannerRepository
    {
        private readonly AppDbContext _context;

        public BannerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Banner?> GetByIdAsync(int id)
        {
            return await _context.Banners.FindAsync(id);
        }

        public async Task<List<Banner>> GetAllAsync()
        {
            return await _context.Banners
                .AsNoTracking()
                .OrderBy(b => b.Position)
                .ThenBy(b => b.SortOrder)
                .ToListAsync();
        }

        public async Task<List<Banner>> GetByPositionAsync(string position)
        {
            return await _context.Banners
                .AsNoTracking()
                .Where(b => b.Position == position)
                .OrderBy(b => b.SortOrder)
                .ToListAsync();
        }

        public async Task<List<Banner>> GetActiveByPositionAsync(string position)
        {
            var now = DateTime.UtcNow;
            return await _context.Banners
                .AsNoTracking()
                .Where(b => b.Position == position && b.IsActive)
                .Where(b => b.StartDate == null || b.StartDate <= now)
                .Where(b => b.EndDate == null || b.EndDate >= now)
                .OrderBy(b => b.SortOrder)
                .ToListAsync();
        }

        public async Task AddAsync(Banner banner)
        {
            await _context.Banners.AddAsync(banner);
        }

        public void Update(Banner banner)
        {
            _context.Banners.Update(banner);
        }

        public void Delete(Banner banner)
        {
            _context.Banners.Remove(banner);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Banners.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
