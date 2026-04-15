using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IProductCommentService
    {
        Task<List<ProductCommentResponse>> GetAllAsync();
        Task<List<ProductCommentResponse>> GetByProductAsync(int productId);
        Task<List<ProductCommentResponse>> GetByUserAsync(int userId);
        Task<List<ProductCommentResponse>> GetByOrderAsync(int orderId);
        Task<ProductCommentResponse?> GetByProductAndOrderAsync(int productId, int orderId);
        Task<List<ProductCommentResponse>> GetPendingApprovalAsync();
        Task<ProductCommentResponse?> GetByIdAsync(int id);
        Task<int> CountAsync();
        Task<int> CountPendingAsync();
        Task<ProductCommentResponse> CreateAsync(CreateProductCommentRequest request);
        Task<ProductCommentResponse> AddReplyAsync(int parentCommentId, int userId, int orderId, string content, int rating, bool isVerifiedPurchase = false);
        Task<ProductCommentResponse> UpdateAsync(int id, CreateProductCommentRequest request);
        Task ApproveAsync(int id);
        Task RejectAsync(int id);
        Task DeleteAsync(int id);
        Task DeleteByIdAsync(int id);
    }
}
