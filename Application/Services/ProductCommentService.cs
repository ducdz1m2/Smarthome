using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;

namespace Application.Services
{
    public class ProductCommentService : IProductCommentService
    {
        private readonly IProductCommentRepository _commentRepository;
        private readonly IProductRepository _productRepository;

        public ProductCommentService(IProductCommentRepository commentRepository, IProductRepository productRepository)
        {
            _commentRepository = commentRepository;
            _productRepository = productRepository;
        }

        public async Task<List<ProductCommentResponse>> GetAllAsync()
        {
            var comments = await _commentRepository.GetAllAsync();
            return comments.Select(MapToResponse).ToList();
        }

        public async Task<List<ProductCommentResponse>> GetByProductAsync(int productId)
        {
            var comments = await _commentRepository.GetByProductAsync(productId);
            return comments.Select(MapToResponse).ToList();
        }

        public async Task<List<ProductCommentResponse>> GetByUserAsync(int userId)
        {
            var comments = await _commentRepository.GetByUserAsync(userId);
            return comments.Select(MapToResponse).ToList();
        }

        public async Task<List<ProductCommentResponse>> GetByOrderAsync(int orderId)
        {
            var comments = await _commentRepository.GetByOrderAsync(orderId);
            return comments.Select(MapToResponse).ToList();
        }

        public async Task<ProductCommentResponse?> GetByProductAndOrderAsync(int productId, int orderId)
        {
            var comment = await _commentRepository.GetByProductAndOrderAsync(productId, orderId);
            return comment == null ? null : MapToResponse(comment);
        }

        public async Task<List<ProductCommentResponse>> GetPendingApprovalAsync()
        {
            var comments = await _commentRepository.GetPendingApprovalAsync();
            return comments.Select(MapToResponse).ToList();
        }

        public async Task<ProductCommentResponse?> GetByIdAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            return comment == null ? null : MapToResponse(comment);
        }

        public async Task<int> CountAsync()
        {
            return await _commentRepository.CountAsync();
        }

        public async Task<int> CountPendingAsync()
        {
            return await _commentRepository.CountPendingAsync();
        }

        public async Task<ProductCommentResponse> CreateAsync(CreateProductCommentRequest request)
        {
            var comment = ProductComment.Create(
                request.ProductId,
                request.UserId,
                request.OrderId,
                request.Content,
                request.Rating,
                request.IsVerifiedPurchase);

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();

            // Reload with Product for mapping
            var created = await _commentRepository.GetByIdAsync(comment.Id);
            return MapToResponse(created!);
        }

        public async Task<ProductCommentResponse> AddReplyAsync(int parentCommentId, int userId, int orderId, string content, int rating, bool isVerifiedPurchase = false)
        {
            var parentComment = await _commentRepository.GetByIdAsync(parentCommentId);
            if (parentComment == null)
                throw new Exception("Không tìm thấy đánh giá cha");

            // Use OrderId from parent comment if not provided
            var actualOrderId = orderId > 0 ? orderId : parentComment.OrderId;

            var reply = ProductComment.Create(
                parentComment.ProductId,
                userId,
                actualOrderId,
                content,
                rating,
                isVerifiedPurchase,
                parentCommentId);

            await _commentRepository.AddAsync(reply);
            await _commentRepository.SaveChangesAsync();

            // Reload with Product for mapping
            var created = await _commentRepository.GetByIdAsync(reply.Id);
            return MapToResponse(created!);
        }

        public async Task ApproveAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return;
            
            comment.Approve();
            _commentRepository.Update(comment);
            await _commentRepository.SaveChangesAsync();
        }

        public async Task RejectAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return;
            
            comment.Reject();
            _commentRepository.Update(comment);
            await _commentRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return;
            
            _commentRepository.Delete(comment);
            await _commentRepository.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            await _commentRepository.DeleteByIdAsync(id);
        }

        private ProductCommentResponse MapToResponse(ProductComment comment)
        {
            return new ProductCommentResponse
            {
                Id = comment.Id,
                ProductId = comment.ProductId,
                ProductName = comment.Product?.Name ?? $"Product #{comment.ProductId}",
                UserId = comment.UserId,
                UserName = $"User #{comment.UserId}", // TODO: Get actual user name
                Content = comment.Content,
                Rating = comment.Rating,
                IsApproved = comment.IsApproved,
                IsVerifiedPurchase = comment.IsVerifiedPurchase,
                CreatedAt = comment.CreatedAt,
                Replies = comment.Replies.Select(MapToResponse).ToList()
            };
        }
    }
}
