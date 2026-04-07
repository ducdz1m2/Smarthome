using Domain.Entities.Promotions;

namespace Application.Interfaces.Repositories
{
    public interface IPromotionRepository
    {
        Task<Promotion?> GetByIdAsync(int id);
        Task<Promotion?> GetByIdWithProductsAsync(int id);
        Task<List<Promotion>> GetAllAsync();
        Task<List<Promotion>> GetActiveAsync();
        Task<List<Promotion>> GetActiveForProductAsync(int productId);
        Task AddAsync(Promotion promotion);
        void Update(Promotion promotion);
        void Delete(Promotion promotion);
        Task<bool> ExistsAsync(string name, int? excludeId = null);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
