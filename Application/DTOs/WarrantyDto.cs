namespace Application.DTOs
{
    public class WarrantyDto
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int WarrantyPeriodMonths { get; set; }
        public List<WarrantyClaimDto> Claims { get; set; } = new();
    }

    public class WarrantyClaimDto
    {
        public int Id { get; set; }
        public int WarrantyId { get; set; }
        public DateTime ClaimDate { get; set; }
        public string Issue { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
    }

    public class CreateWarrantyClaimRequest
    {
        public int WarrantyId { get; set; }
        public string Issue { get; set; } = string.Empty;
    }

    public class ReturnOrderDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ReturnOrderItemDto> Items { get; set; } = new();
    }

    public class ReturnOrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class CreateReturnOrderRequest
    {
        public int OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<CreateReturnOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateReturnOrderItemRequest
    {
        public int OrderItemId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
