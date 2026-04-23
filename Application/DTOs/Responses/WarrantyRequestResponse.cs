namespace Application.DTOs.Responses;

using Domain.Enums;

public class WarrantyRequestResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int? WarrantyId { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int OrderItemId { get; set; }
    public int? InstallationBookingId { get; set; }
    public int? AssignedTechnicianId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
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
    public string Description { get; set; }
    public bool IsDamaged { get; set; }
    public bool ReturnedToInventory { get; set; }
    public DamagedProductStatus DamagedStatus { get; set; }
    public int? WarehouseId { get; set; }
    public decimal? RepairCost { get; set; }
    public string? RepairNotes { get; set; }
}
