

using Domain.Entities.Common;
using Domain.Exceptions;

namespace Domain.Entities.Catalog
{
    public class ProductComment : BaseEntity
    {
        public int ProductId { get; private set; }
        public int UserId { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public int Rating { get; private set; } // 1-5 stars
        public bool IsApproved { get; private set; } = false;
        public bool IsVerifiedPurchase { get; private set; } = false; // Đã mua hàng mới được đánh giá
        public int? ParentCommentId { get; private set; } // Trả lời comment

        // Navigation
        public virtual Product Product { get; private set; } = null!;
        public virtual ProductComment? ParentComment { get; private set; }
        public virtual ICollection<ProductComment> Replies { get; private set; } = new List<ProductComment>();

        private ProductComment() { } // EF Core

        public static ProductComment Create(int productId, int userId, string content, int rating, bool isVerifiedPurchase = false, int? parentCommentId = null)
        {
            if (productId <= 0)
                throw new ValidationException(nameof(productId), "ProductId không hợp lệ");

            if (userId <= 0)
                throw new ValidationException(nameof(userId), "UserId không hợp lệ");

            if (string.IsNullOrWhiteSpace(content))
                throw new ValidationException(nameof(content), "Nội dung đánh giá không được trống");

            if (content.Length > 1000)
                throw new ValidationException(nameof(content), "Nội dung đánh giá tối đa 1000 ký tự");

            if (rating < 1 || rating > 5)
                throw new ValidationException(nameof(rating), "Đánh giá phải từ 1-5 sao");

            return new ProductComment
            {
                ProductId = productId,
                UserId = userId,
                Content = content.Trim(),
                Rating = rating,
                IsVerifiedPurchase = isVerifiedPurchase,
                ParentCommentId = parentCommentId,
                IsApproved = false // Chờ admin duyệt
            };
        }

        public void Approve()
        {
            IsApproved = true;
        }

        public void Reject()
        {
            IsApproved = false;
        }

        public void UpdateContent(string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
                throw new ValidationException(nameof(newContent), "Nội dung không được trống");

            if (newContent.Length > 1000)
                throw new ValidationException(nameof(newContent), "Nội dung tối đa 1000 ký tự");

            Content = newContent.Trim();

            // Reset approval khi sửa
            IsApproved = false;
        }

        public void MarkAsVerifiedPurchase()
        {
            IsVerifiedPurchase = true;
        }

        public void AddReply(int userId, string content, int rating, bool isVerifiedPurchase = false)
        {
            var reply = Create(ProductId, userId, content, rating, isVerifiedPurchase, Id);
            Replies.Add(reply);
        }
    }
}
