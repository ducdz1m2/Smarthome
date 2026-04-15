using Domain.Abstractions;
using Domain.Events;
using Domain.Exceptions;

namespace Domain.Entities.Installation;

/// <summary>
/// TechnicianRating entity - represents a customer review/rating on a technician.
/// </summary>
public class TechnicianRating : Entity
{
    public int TechnicianId { get; private set; }
    public int UserId { get; private set; }
    public int BookingId { get; private set; } // Liên kết với lịch lắp đặt cụ thể
    public string Content { get; private set; } = string.Empty;
    public int Rating { get; private set; } // 1-5 stars
    public bool IsApproved { get; private set; } = false;
    public bool IsVerifiedService { get; private set; } = false; // Đã sử dụng dịch vụ mới được đánh giá

    // Navigation
    public virtual TechnicianProfile Technician { get; private set; } = null!;
    public virtual InstallationBooking Booking { get; private set; } = null!;

    private TechnicianRating() { } // EF Core

    public static TechnicianRating Create(int technicianId, int userId, int bookingId, string content, int rating, bool isVerifiedService = false)
    {
        if (technicianId <= 0)
            throw new ValidationException(nameof(technicianId), "TechnicianId không hợp lệ");

        if (userId <= 0)
            throw new ValidationException(nameof(userId), "UserId không hợp lệ");

        if (bookingId <= 0)
            throw new ValidationException(nameof(bookingId), "BookingId không hợp lệ");

        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException(nameof(content), "Nội dung đánh giá không được trống");

        if (content.Length > 1000)
            throw new ValidationException(nameof(content), "Nội dung đánh giá tối đa 1000 ký tự");

        if (rating < 1 || rating > 5)
            throw new ValidationException(nameof(rating), "Đánh giá phải từ 1-5 sao");

        var ratingEntity = new TechnicianRating
        {
            TechnicianId = technicianId,
            UserId = userId,
            BookingId = bookingId,
            Content = content.Trim(),
            Rating = rating,
            IsVerifiedService = isVerifiedService,
            IsApproved = false // Chờ admin duyệt
        };
        
        ratingEntity.AddDomainEvent(new TechnicianRatingCreatedEvent(
            ratingEntity.Id,
            ratingEntity.TechnicianId,
            ratingEntity.UserId,
            ratingEntity.Content.Trim(),
            ratingEntity.Rating
        ));

        return ratingEntity;
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

    public void UpdateRating(int newRating)
    {
        if (newRating < 1 || newRating > 5)
            throw new ValidationException(nameof(newRating), "Đánh giá phải từ 1-5 sao");

        Rating = newRating;

        // Reset approval khi sửa
        IsApproved = false;
    }

    public void MarkAsVerifiedService()
    {
        IsVerifiedService = true;
    }
}
