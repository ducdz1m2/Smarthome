using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IWarrantyService
    {
        // Warranty methods
        Task<List<WarrantyResponse>> GetAllAsync();
        Task<WarrantyResponse?> GetByIdAsync(int id);
        Task<WarrantyResponse?> GetByIdWithClaimsAsync(int id);
        Task<List<WarrantyResponse>> GetByProductIdAsync(int productId);
        Task<List<WarrantyResponse>> GetActiveWarrantiesAsync();
        Task<int> CreateAsync(int orderItemId, int productId, int durationInMonths);
        Task ExtendWarrantyAsync(int id, int additionalMonths);
        Task UpdateStatusAsync(int id, string status);
        Task DeleteAsync(int id);

        // Claim methods
        Task<List<WarrantyClaimRepsonse>> GetClaimsByWarrantyIdAsync(int warrantyId);
        Task<List<WarrantyClaimRepsonse>> GetPendingClaimsAsync();
        Task<WarrantyClaimRepsonse?> GetClaimByIdAsync(int id);
        Task<int> CreateClaimAsync(int warrantyId, string issue);
        Task AssignTechnicianAsync(int claimId, int technicianId);
        Task ResolveClaimAsync(int claimId, string resolution, bool isApproved);
        Task ApproveReplacementAsync(int claimId);
    }
}
