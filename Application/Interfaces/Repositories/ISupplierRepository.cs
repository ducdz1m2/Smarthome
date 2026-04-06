using Domain.Entities.Inventory;

namespace Application.Interfaces.Repositories
{
    public interface ISupplierRepository
    {
        Task<Supplier?> GetByIdAsync(int id);
        Task<Supplier?> GetByIdWithStockEntriesAsync(int id);
        Task<List<Supplier>> GetAllAsync();
        Task<List<Supplier>> GetActiveAsync();
        Task AddAsync(Supplier supplier);
        void Update(Supplier supplier);
        void Delete(Supplier supplier);
        Task<bool> ExistsAsync(string name, int? excludeId = null);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
