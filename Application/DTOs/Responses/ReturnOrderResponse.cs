namespace Application.DTOs.Responses
{


    public class ReturnOrderResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
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
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
