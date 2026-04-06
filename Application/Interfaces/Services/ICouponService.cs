using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface ICouponService
    {
        Task<List<CouponResponse>> GetAllAsync();
        Task<CouponResponse?> GetByIdAsync(int id);
        Task<CouponResponse?> GetByCodeAsync(string code);
        Task<int> CreateAsync(CreateCouponRequest request);
        Task UpdateAsync(int id, UpdateCouponRequest request);
        Task DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
