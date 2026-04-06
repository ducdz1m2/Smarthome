using Domain.Entities.Catalog;

namespace Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetByIdWithChildrenAsync(int id);
        Task<List<Category>> GetAllAsync();
        Task<List<Category>> GetActiveAsync();
        Task<List<Category>> GetRootCategoriesAsync();
        Task AddAsync(Category category);
        void Update(Category category);
        void Delete(Category category);
        Task<bool> ExistsAsync(string name, int? excludeId = null);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
