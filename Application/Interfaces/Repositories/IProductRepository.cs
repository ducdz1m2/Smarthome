using Domain.Entities.Catalog;

namespace Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<List<Product>> GetAllAsync();
        Task<List<Product>> GetByCategoryAsync(int categoryId);
        Task AddAsync(Product product);
        Task DeleteAsync(Product product);
        Task SaveChangesAsync();
        Task<List<Product>> SearchAsync(string keyword, int? categoryId);
    }
}
