namespace Domain.Entities.Sales;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Exceptions;

/// <summary>
/// ProductRating entity - represents a rating for a specific product instance.
/// </summary>
public class ProductRating : AggregateRoot
{
    public int ProductId { get; private set; }
    public int? VariantId { get; private set; }
    public int OrderItemId { get; private set; }
    public int CustomerId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTime RatedAt { get; private set; }
    public RatingStatus Status { get; private set; } = RatingStatus.Approved;

    private ProductRating() { }

    public static ProductRating Create(int productId, int? variantId, int orderItemId, int customerId, int rating, string? comment = null)
    {
        if (rating < 1 || rating > 5)
            throw new ValidationException(nameof(rating), "Đánh giá phải từ 1 đến 5 sao");

        if (orderItemId <= 0)
            throw new ValidationException(nameof(orderItemId), "OrderItemId không hợp lệ");

        return new ProductRating
        {
            ProductId = productId,
            VariantId = variantId,
            OrderItemId = orderItemId,
            CustomerId = customerId,
            Rating = rating,
            Comment = comment?.Trim(),
            RatedAt = DateTime.UtcNow,
            Status = RatingStatus.Approved
        };
    }

    public void UpdateRating(int rating, string? comment = null)
    {
        if (rating < 1 || rating > 5)
            throw new ValidationException(nameof(rating), "Đánh giá phải từ 1 đến 5 sao");

        Rating = rating;
        Comment = comment?.Trim();
    }

    public void Moderate(bool approved)
    {
        Status = approved ? RatingStatus.Approved : RatingStatus.Rejected;
    }
}

public enum RatingStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
