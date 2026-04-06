using Domain.Entities.Inventory;

namespace Application.Interfaces.Repositories
{
    public interface IWarehouseRepository
    {
        Task<Warehouse?> GetByIdAsync(int id);
        Task<List<Warehouse>> GetAllAsync();
        Task<List<Warehouse>> GetActiveAsync();
        Task AddAsync(Warehouse warehouse);
        void Update(Warehouse warehouse);
        void Delete(Warehouse warehouse);
        Task<bool> ExistsAsync(string name, int? excludeId = null);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
