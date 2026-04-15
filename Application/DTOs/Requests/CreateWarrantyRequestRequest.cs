namespace Application.DTOs.Requests;

using Domain.Enums;

public class CreateWarrantyRequestRequest
{
    public int OrderId { get; set; }
    public WarrantyType WarrantyType { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<WarrantyRequestItemDto> Items { get; set; } = new();
}

public class WarrantyRequestItemDto
{
    public int OrderItemId { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
}
