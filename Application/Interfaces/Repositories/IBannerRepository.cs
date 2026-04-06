using Domain.Entities.Content;

namespace Application.Interfaces.Repositories
{
    public interface IBannerRepository
    {
        Task<Banner?> GetByIdAsync(int id);
        Task<List<Banner>> GetAllAsync();
        Task<List<Banner>> GetByPositionAsync(string position);
        Task<List<Banner>> GetActiveByPositionAsync(string position);
        Task AddAsync(Banner banner);
        void Update(Banner banner);
        void Delete(Banner banner);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
