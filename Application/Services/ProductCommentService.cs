using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Enums;

namespace Application.Services
{
    public class ProductCommentService : IProductCommentService
    {
        private readonly IProductCommentRepository _commentRepository;
        private readonly IProductRepository _productRepository;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly Domain.Repositories.IUserRepository _userRepository;

        public ProductCommentService(IProductCommentRepository commentRepository, IProductRepository productRepository, INotificationService notificationService, IEmailService emailService, Domain.Repositories.IUserRepository userRepository)
        {
            _commentRepository = commentRepository;
            _productRepository = productRepository;
            _notificationService = notificationService;
            _emailService = emailService;
            _userRepository = userRepository;
        }

        public async Task<List<ProductCommentResponse>> GetAllAsync()
        {
            var comments = await _commentRepository.GetAllAsync();
            var result = new List<ProductCommentResponse>();
            foreach (var comment in comments)
            {
                result.Add(await MapToResponseAsync(comment));
            }
            return result;
        }

        public async Task<List<ProductCommentResponse>> GetByProductAsync(int productId)
        {
            var comments = await _commentRepository.GetByProductAsync(productId);
            var result = new List<ProductCommentResponse>();
            foreach (var comment in comments)
            {
                result.Add(await MapToResponseAsync(comment));
            }
            return result;
        }

        public async Task<List<ProductCommentResponse>> GetByUserAsync(int userId)
        {
            var comments = await _commentRepository.GetByUserAsync(userId);
            var result = new List<ProductCommentResponse>();
            foreach (var comment in comments)
            {
                result.Add(await MapToResponseAsync(comment));
            }
            return result;
        }

        public async Task<List<ProductCommentResponse>> GetByOrderAsync(int orderId)
        {
            var comments = await _commentRepository.GetByOrderAsync(orderId);
            var result = new List<ProductCommentResponse>();
            foreach (var comment in comments)
            {
                result.Add(await MapToResponseAsync(comment));
            }
            return result;
        }

        public async Task<ProductCommentResponse?> GetByProductAndOrderAsync(int productId, int orderId)
        {
            var comment = await _commentRepository.GetByProductAndOrderAsync(productId, orderId);
            return comment == null ? null : await MapToResponseAsync(comment);
        }

        public async Task<List<ProductCommentResponse>> GetPendingApprovalAsync()
        {
            var comments = await _commentRepository.GetPendingApprovalAsync();
            var result = new List<ProductCommentResponse>();
            foreach (var comment in comments)
            {
                result.Add(await MapToResponseAsync(comment));
            }
            return result;
        }

        public async Task<ProductCommentResponse?> GetByIdAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            return comment == null ? null : await MapToResponseAsync(comment);
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
            return await MapToResponseAsync(created!);
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
            return await MapToResponseAsync(created!);
        }

        public async Task<ProductCommentResponse> UpdateAsync(int id, CreateProductCommentRequest request)
        {
            var comment = await _commentRepository.GetByIdForUpdateAsync(id);
            if (comment == null)
                throw new Exception("Không tìm thấy đánh giá");

            comment.UpdateContent(request.Content);
            
            // Update rating using reflection since there's no UpdateRating method
            comment.GetType().GetProperty("Rating")?.SetValue(comment, request.Rating);
            
            // Reset approval when editing
            comment.GetType().GetProperty("IsApproved")?.SetValue(comment, false);

            _commentRepository.Update(comment);
            await _commentRepository.SaveChangesAsync();

            // Reload with Product for mapping
            var updated = await _commentRepository.GetByIdAsync(id);
            return await MapToResponseAsync(updated!);
        }

        public async Task ApproveAsync(int id)
        {
            var comment = await _commentRepository.GetByIdForUpdateAsync(id);
            if (comment == null) return;

            comment.Approve();
            _commentRepository.Update(comment);
            await _commentRepository.SaveChangesAsync();

            // Send notification to user
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                UserId = comment.UserId,
                UserType = UserType.Customer,
                Type = NotificationType.OrderConfirmed,
                Title = "Đánh giá sản phẩm đã được duyệt",
                Message = "Đánh giá sản phẩm của bạn đã được duyệt và hiển thị công khai.",
                ActionUrl = $"/products/{comment.ProductId}",
                Icon = "check-circle",
                RelatedEntityId = comment.Id,
                RelatedEntityType = "ProductComment"
            });

            // Send email
            var user = await _userRepository.GetByIdAsync(comment.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendRatingApprovedEmailAsync(user.Email, "ProductComment", comment.Id);
            }
        }

        public async Task RejectAsync(int id)
        {
            var comment = await _commentRepository.GetByIdForUpdateAsync(id);
            if (comment == null) return;
            
            _commentRepository.Delete(comment);
            await _commentRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _commentRepository.GetByIdForUpdateAsync(id);
            if (comment == null) return;
            
            _commentRepository.Delete(comment);
            await _commentRepository.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            await _commentRepository.DeleteByIdAsync(id);
        }

        private async Task<ProductCommentResponse> MapToResponseAsync(ProductComment comment)
        {
            var user = await _userRepository.GetByIdAsync(comment.UserId);
            var userName = user?.FullName ?? user?.UserName ?? $"User #{comment.UserId}";

            var replies = new List<ProductCommentResponse>();
            foreach (var reply in comment.Replies)
            {
                replies.Add(await MapToResponseAsync(reply));
            }

            return new ProductCommentResponse
            {
                Id = comment.Id,
                ProductId = comment.ProductId,
                ProductName = comment.Product?.Name ?? $"Product #{comment.ProductId}",
                UserId = comment.UserId,
                UserName = userName,
                Content = comment.Content,
                Rating = comment.Rating,
                IsApproved = comment.IsApproved,
                IsVerifiedPurchase = comment.IsVerifiedPurchase,
                CreatedAt = comment.CreatedAt,
                Replies = replies
            };
        }
    }
}
