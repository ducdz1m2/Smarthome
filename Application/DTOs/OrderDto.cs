namespace Application.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverPhone { get; set; }
        public string? ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = string.Empty;
        public string? CancelReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<OrderShipmentDto> Shipments { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool RequiresInstallation { get; set; }
    }

    public class OrderShipmentDto
    {
        public int Id { get; set; }
        public string ShipmentNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ShippingStreet { get; set; } = string.Empty;
        public string ShippingWard { get; set; } = string.Empty;
        public string ShippingDistrict { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? VariantId { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
