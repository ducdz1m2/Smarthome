namespace Application.DTOs.Requests;

public class CreateWarrantyRequestRequest
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int OrderItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
}
