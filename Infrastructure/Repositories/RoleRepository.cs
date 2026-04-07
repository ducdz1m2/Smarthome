using Application.Interfaces.Repositories;
using Domain.Entities.Identity;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AppRole?> GetByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<AppRole?> GetByNameAsync(string name)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<List<AppRole>> GetAllAsync()
        {
            return await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task AddAsync(AppRole role)
        {
            await _context.Roles.AddAsync(role);
        }

        public void Update(AppRole role)
        {
            _context.Roles.Update(role);
        }

        public void Delete(AppRole role)
        {
            _context.Roles.Remove(role);
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Roles.AnyAsync(r => r.Name == name);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Roles.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
