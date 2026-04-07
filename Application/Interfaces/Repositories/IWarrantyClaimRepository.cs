using Domain.Entities.Sales;

namespace Application.Interfaces.Repositories
{
    public interface IWarrantyClaimRepository
    {
        Task<WarrantyClaim?> GetByIdAsync(int id);
        Task<List<WarrantyClaim>> GetAllAsync();
        Task<List<WarrantyClaim>> GetByWarrantyIdAsync(int warrantyId);
        Task<List<WarrantyClaim>> GetByStatusAsync(WarrantyClaimStatus status);
        Task<List<WarrantyClaim>> GetPendingClaimsAsync();
        Task AddAsync(WarrantyClaim claim);
        void Update(WarrantyClaim claim);
        void Delete(WarrantyClaim claim);
        Task<int> CountAsync();
        Task<int> CountByStatusAsync(WarrantyClaimStatus status);
        Task SaveChangesAsync();
    }
}
