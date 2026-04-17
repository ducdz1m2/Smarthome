namespace Application.DTOs.ML;

/// <summary>
/// DTO dùng cho prediction result từ ML.NET
/// </summary>
public class ProductPredictionDto
{
    public float Score { get; set; }
    public uint ProductId { get; set; }
}
