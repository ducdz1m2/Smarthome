using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IPromotionService
    {
        Task<List<PromotionResponse>> GetAllAsync();
        Task<PromotionResponse?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreatePromotionRequest request);
        Task UpdateAsync(int id, UpdatePromotionRequest request);
        Task DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
        Task<List<PromotionResponse>> GetActiveAsync();
        Task<List<PromotionResponse>> GetActiveForProductAsync(int productId);
        Task<decimal> CalculateDiscountAsync(int promotionId, decimal originalPrice, int? productId = null);
    }
}
