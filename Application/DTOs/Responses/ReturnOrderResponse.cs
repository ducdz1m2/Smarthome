namespace Application.DTOs.Responses;

using Domain.Enums;

    public class ReturnOrderResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        public string ReturnMethod { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ReturnOrderItemDto> Items { get; set; } = new();
    }

    public class ReturnOrderItemDto
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsDamaged { get; set; }
        public bool ReturnedToInventory { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public DamagedProductStatus DamagedStatus { get; set; }
        public decimal? RepairCost { get; set; }
        public string? RepairNotes { get; set; }
    }
