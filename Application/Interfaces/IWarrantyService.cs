using Application.DTOs;

namespace Application.Interfaces
{
    public interface IWarrantyService
    {
        Task<List<WarrantyDto>> GetByUserAsync(int userId);
        Task<WarrantyDto?> GetByIdAsync(int id);
        
        Task<int> CreateClaimAsync(CreateWarrantyClaimRequest request);
        Task<List<WarrantyClaimDto>> GetClaimsAsync(int warrantyId);
        Task AssignTechnicianToClaimAsync(int claimId, int technicianId);
        Task ResolveClaimAsync(int claimId, string resolution, bool isApproved);
        
        Task<List<ReturnOrderDto>> GetReturnOrdersAsync(int? userId = null);
        Task<ReturnOrderDto?> GetReturnOrderByIdAsync(int id);
        Task<int> CreateReturnOrderAsync(CreateReturnOrderRequest request);
        Task ProcessReturnAsync(int returnOrderId, bool accept, decimal? refundAmount);
    }
}
