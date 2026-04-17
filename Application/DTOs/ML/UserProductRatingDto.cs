namespace Application.DTOs.ML;

/// <summary>
/// DTO dùng cho training ML.NET - Matrix Factorization
/// </summary>
public class UserProductRatingDto
{
    public uint UserId { get; set; }
    public uint ProductId { get; set; }
    public float Rating { get; set; }
}
