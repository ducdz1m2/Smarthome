using Domain.Entities.Installation;

namespace Application.Interfaces.Repositories
{
    public interface ITechnicianProfileRepository
    {
        Task<TechnicianProfile?> GetByIdAsync(int id);
        Task<TechnicianProfile?> GetByUserIdAsync(int userId);
        Task<TechnicianProfile?> GetByIdWithSlotsAsync(int id);
        Task<List<TechnicianProfile>> GetAllAsync();
        Task<List<TechnicianProfile>> GetAvailableAsync();
        Task<List<TechnicianProfile>> GetByDistrictAsync(string district);
        Task<(List<TechnicianProfile> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, bool? isAvailable = null, string? search = null);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByUserIdAsync(int userId);
        Task<bool> ExistsByEmployeeCodeAsync(string employeeCode);
        Task AddAsync(TechnicianProfile technician);
        void Update(TechnicianProfile technician);
        void Delete(TechnicianProfile technician);
        Task<int> CountAsync();
        Task SaveChangesAsync();
    }
}
