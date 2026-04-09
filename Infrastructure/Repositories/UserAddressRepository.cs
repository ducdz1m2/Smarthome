using Application.Interfaces.Repositories;
using Domain.Entities.Content;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserAddressRepository : IUserAddressRepository
    {
        private readonly AppDbContext _context;

        public UserAddressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserAddress?> GetByIdAsync(int id)
        {
            return await _context.UserAddresses.FindAsync(id);
        }

        public async Task<List<UserAddress>> GetByUserIdAsync(int userId)
        {
            return await _context.UserAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
        }

        public async Task<UserAddress?> GetDefaultByUserIdAsync(int userId)
        {
            return await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
        }

        public async Task AddAsync(UserAddress address)
        {
            await _context.UserAddresses.AddAsync(address);
        }

        public void Update(UserAddress address)
        {
            _context.UserAddresses.Update(address);
        }

        public void Delete(UserAddress address)
        {
            _context.UserAddresses.Remove(address);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
