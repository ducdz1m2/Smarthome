using Domain.Entities.Identity;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByIdAsync(int id);
        Task<AppUser?> GetByUserNameAsync(string userName);
        Task<AppUser?> GetByEmailAsync(string email);
        Task<AppUser?> GetByIdWithRolesAsync(int id);
        Task<List<AppUser>> GetAllAsync();
        Task<List<AppUser>> GetByRoleAsync(string roleName);
        Task AddAsync(AppUser user);
        void Update(AppUser user);
        void Delete(AppUser user);
        Task<bool> ExistsAsync(string userName);
        Task<bool> ExistsEmailAsync(string email);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
