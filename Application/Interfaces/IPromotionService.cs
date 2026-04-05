using Application.DTOs;

namespace Application.Interfaces
{
    public interface IPromotionService
    {
        Task<List<CouponDto>> GetAllCouponsAsync();
        Task<CouponDto?> GetCouponByCodeAsync(string code);
        Task<int> CreateCouponAsync(CreateCouponRequest request);
        Task UpdateCouponAsync(int id, CreateCouponRequest request);
        Task DeleteCouponAsync(int id);
        Task<ApplyCouponResult> ApplyCouponAsync(ApplyCouponRequest request);
        
        Task<List<PromotionDto>> GetActivePromotionsAsync();
    }
}
