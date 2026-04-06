using Domain.Entities.Catalog;

namespace Application.Interfaces.Repositories
{
    public interface IBrandRepository
    {
        Task<Brand?> GetByIdAsync(int id);
        Task<Brand?> GetByIdWithProductsAsync(int id);
        Task<List<Brand>> GetAllAsync();
        Task<List<Brand>> GetActiveAsync();
        Task AddAsync(Brand brand);
        void Update(Brand brand);
        void Delete(Brand brand);
        Task<bool> ExistsAsync(string name, int? excludeId = null);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
