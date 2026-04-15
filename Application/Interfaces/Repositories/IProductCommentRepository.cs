using Domain.Entities.Catalog;

namespace Application.Interfaces.Repositories
{
    public interface IProductCommentRepository
    {
        Task<ProductComment?> GetByIdAsync(int id);
        Task<ProductComment?> GetByIdForUpdateAsync(int id);
        Task<List<ProductComment>> GetAllAsync();
        Task<List<ProductComment>> GetByProductAsync(int productId);
        Task<List<ProductComment>> GetByUserAsync(int userId);
        Task<List<ProductComment>> GetByOrderAsync(int orderId);
        Task<ProductComment?> GetByProductAndOrderAsync(int productId, int orderId);
        Task<List<ProductComment>> GetPendingApprovalAsync();
        Task<int> CountAsync();
        Task<int> CountPendingAsync();
        Task AddAsync(ProductComment comment);
        void Update(ProductComment comment);
        void Delete(ProductComment comment);
        Task DeleteByIdAsync(int id);
        Task SaveChangesAsync();
    }
}
