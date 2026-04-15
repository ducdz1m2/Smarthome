namespace Application.DTOs.Responses;

public class WarrantyRequestResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string WarrantyType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? TechnicianNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WarrantyRequestItemResponseDto> Items { get; set; } = new();
}

public class WarrantyRequestItemResponseDto
{
    public int Id { get; set; }
    public int OrderItemId { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
}
