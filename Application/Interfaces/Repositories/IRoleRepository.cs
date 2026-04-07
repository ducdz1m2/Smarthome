using Domain.Entities.Identity;

namespace Application.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        Task<AppRole?> GetByIdAsync(int id);
        Task<AppRole?> GetByNameAsync(string name);
        Task<List<AppRole>> GetAllAsync();
        Task AddAsync(AppRole role);
        void Update(AppRole role);
        void Delete(AppRole role);
        Task<bool> ExistsAsync(string name);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
