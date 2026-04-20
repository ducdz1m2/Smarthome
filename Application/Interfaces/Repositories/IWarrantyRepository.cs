using Domain.Entities.Sales;

namespace Application.Interfaces.Repositories
{
    public interface IWarrantyRepository
    {
        Task<Warranty?> GetByIdAsync(int id);
        Task<Warranty?> GetByIdWithClaimsAsync(int id);
        Task<Warranty?> GetByOrderItemIdAsync(int orderItemId);
        Task<List<Warranty>> GetAllAsync();
        Task<List<Warranty>> GetByProductIdAsync(int productId);
        Task<List<Warranty>> GetActiveWarrantiesAsync();
        Task AddAsync(Warranty warranty);
        void Update(Warranty warranty);
        void Delete(Warranty warranty);
        Task<bool> ExistsAsync(int orderItemId);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
