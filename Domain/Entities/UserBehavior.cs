namespace Domain.Entities;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Entities.Catalog;
using Domain.Entities.Identity;

/// <summary>
/// Entity lưu trữ hành vi của người dùng để phục vụ ML.NET recommendation system
/// </summary>
public class UserBehavior : Entity
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public BehaviorType BehaviorType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public float Rating { get; set; } = 0; // Đánh giá từ 0-5, dùng cho collaborative filtering
    public string? AdditionalData { get; set; } // JSON string để lưu thêm thông tin nếu cần

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
