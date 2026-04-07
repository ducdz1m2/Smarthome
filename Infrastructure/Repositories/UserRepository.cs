using Application.Interfaces.Repositories;
using Domain.Entities.Identity;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AppUser?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser?> GetByUserNameAsync(string userName)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == userName.ToLower());
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower());
        }

        public async Task<AppUser?> GetByIdWithRolesAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<AppUser>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AppUser>> GetByRoleAsync(string roleName)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Roles.Any(r => r.Name == roleName))
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(AppUser user)
        {
            await _context.Users.AddAsync(user);
        }

        public void Update(AppUser user)
        {
            _context.Users.Update(user);
        }

        public void Delete(AppUser user)
        {
            _context.Users.Remove(user);
        }

        public async Task<bool> ExistsAsync(string userName)
        {
            return await _context.Users.AnyAsync(u => u.UserName == userName.ToLower());
        }

        public async Task<bool> ExistsEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email.ToLower());
        }

        public async Task<int> CountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
