using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Entities.Sales;
using Domain.Exceptions;

namespace Application.Services
{
    public class WarrantyService : IWarrantyService
    {
        private const int DaysPerMonth = 30;

        private readonly IWarrantyRepository _warrantyRepository;
        private readonly IWarrantyClaimRepository _claimRepository;
        private readonly IProductRepository _productRepository;

        public WarrantyService(
            IWarrantyRepository warrantyRepository,
            IWarrantyClaimRepository claimRepository,
            IProductRepository productRepository)
        {
            _warrantyRepository = warrantyRepository;
            _claimRepository = claimRepository;
            _productRepository = productRepository;
        }

        // Warranty methods
        public async Task<List<WarrantyResponse>> GetAllAsync()
        {
            var warranties = await _warrantyRepository.GetAllAsync();
            var tasks = warranties.Select(MapToResponseAsync);
            var responses = await Task.WhenAll(tasks);
            return responses.ToList();
        }

        public async Task<WarrantyResponse?> GetByIdAsync(int id)
        {
            var warranty = await _warrantyRepository.GetByIdAsync(id);
            if (warranty == null) return null;
            return await MapToResponseAsync(warranty);
        }

        public async Task<WarrantyResponse?> GetByIdWithClaimsAsync(int id)
        {
            var warranty = await _warrantyRepository.GetByIdWithClaimsAsync(id);
            if (warranty == null) return null;
            return await MapToResponseWithClaimsAsync(warranty);
        }

        public async Task<List<WarrantyResponse>> GetByProductIdAsync(int productId)
        {
            var warranties = await _warrantyRepository.GetByProductIdAsync(productId);
            var tasks = warranties.Select(MapToResponseAsync);
            var responses = await Task.WhenAll(tasks);
            return responses.ToList();
        }

        public async Task<List<WarrantyResponse>> GetActiveWarrantiesAsync()
        {
            var warranties = await _warrantyRepository.GetActiveWarrantiesAsync();
            var tasks = warranties.Select(MapToResponseAsync);
            var responses = await Task.WhenAll(tasks);
            return responses.ToList();
        }

        public async Task<int> CreateAsync(int orderItemId, int productId, int durationInMonths)
        {
            if (await _warrantyRepository.ExistsAsync(orderItemId))
                throw new DomainException("Sản phẩm này đã có bảo hành");

            var warranty = Warranty.Create(orderItemId, productId, durationInMonths);
            await _warrantyRepository.AddAsync(warranty);
            await _warrantyRepository.SaveChangesAsync();
            return warranty.Id;
        }

        public async Task ExtendWarrantyAsync(int id, int additionalMonths)
        {
            var warranty = await _warrantyRepository.GetByIdAsync(id);
            if (warranty == null)
                throw new DomainException("Không tìm thấy thông tin bảo hành");

            warranty.Extend(additionalMonths);
            _warrantyRepository.Update(warranty);
            await _warrantyRepository.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            var warranty = await _warrantyRepository.GetByIdAsync(id);
            if (warranty == null)
                throw new DomainException("Không tìm thấy thông tin bảo hành");

            if (!Enum.TryParse<WarrantyStatus>(status, out var warrantyStatus))
                throw new DomainException("Trạng thái không hợp lệ");

            typeof(Warranty).GetProperty("Status")?.SetValue(warranty, warrantyStatus);
            _warrantyRepository.Update(warranty);
            await _warrantyRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var warranty = await _warrantyRepository.GetByIdAsync(id);
            if (warranty == null)
                throw new DomainException("Không tìm thấy thông tin bảo hành");

            _warrantyRepository.Delete(warranty);
            await _warrantyRepository.SaveChangesAsync();
        }

        // Claim methods
        public async Task<List<WarrantyClaimRepsonse>> GetClaimsByWarrantyIdAsync(int warrantyId)
        {
            var claims = await _claimRepository.GetByWarrantyIdAsync(warrantyId);
            return claims.Select(MapToClaimResponse).ToList();
        }

        public async Task<List<WarrantyClaimRepsonse>> GetPendingClaimsAsync()
        {
            var claims = await _claimRepository.GetPendingClaimsAsync();
            return claims.Select(MapToClaimResponse).ToList();
        }

        public async Task<WarrantyClaimRepsonse?> GetClaimByIdAsync(int id)
        {
            var claim = await _claimRepository.GetByIdAsync(id);
            if (claim == null) return null;
            return MapToClaimResponse(claim);
        }

        public async Task<int> CreateClaimAsync(int warrantyId, string issue)
        {
            var warranty = await _warrantyRepository.GetByIdWithClaimsAsync(warrantyId);
            if (warranty == null)
                throw new DomainException("Không tìm thấy thông tin bảo hành");

            var claim = warranty.CreateClaim(issue);
            await _claimRepository.SaveChangesAsync();
            return claim.Id;
        }

        public async Task AssignTechnicianAsync(int claimId, int technicianId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null)
                throw new DomainException("Không tìm thấy yêu cầu bảo hành");

            claim.AssignTechnician(technicianId);
            _claimRepository.Update(claim);
            await _claimRepository.SaveChangesAsync();
        }

        public async Task ResolveClaimAsync(int claimId, string resolution, bool isApproved)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null)
                throw new DomainException("Không tìm thấy yêu cầu bảo hành");

            claim.Resolve(resolution, isApproved);
            _claimRepository.Update(claim);
            await _claimRepository.SaveChangesAsync();
        }

        public async Task ApproveReplacementAsync(int claimId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null)
                throw new DomainException("Không tìm thấy yêu cầu bảo hành");

            claim.ApproveReplacement();
            _claimRepository.Update(claim);
            await _claimRepository.SaveChangesAsync();
        }

        // Mapping methods
        private async Task<WarrantyResponse> MapToResponseAsync(Warranty warranty)
        {
            var product = await _productRepository.GetByIdAsync(warranty.ProductId);
            return new WarrantyResponse
            {
                Id = warranty.Id,
                OrderItemId = warranty.OrderItemId,
                ProductId = warranty.ProductId,
                ProductName = product?.Name ?? "Unknown",
                SerialNumber = "",
                StartDate = warranty.StartDate,
                EndDate = warranty.EndDate,
                Status = warranty.Status.ToString(),
                WarrantyPeriodMonths = (warranty.EndDate - warranty.StartDate).Days / DaysPerMonth,
                Claims = new List<WarrantyClaimRepsonse>()
            };
        }

        private async Task<WarrantyResponse> MapToResponseWithClaimsAsync(Warranty warranty)
        {
            var response = await MapToResponseAsync(warranty);
            response.Claims = warranty.Claims?.Select(MapToClaimResponse).ToList() ?? new List<WarrantyClaimRepsonse>();
            return response;
        }

        private static WarrantyClaimRepsonse MapToClaimResponse(WarrantyClaim claim)
        {
            return new WarrantyClaimRepsonse
            {
                Id = claim.Id,
                WarrantyId = claim.WarrantyId,
                ClaimDate = claim.ClaimDate,
                Issue = claim.Issue,
                Resolution = claim.Resolution,
                Status = claim.Status.ToString(),
                TechnicianId = claim.TechnicianId
            };
        }
    }
}
