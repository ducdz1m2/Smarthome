namespace Application.DTOs.Responses
{
    public class OrderResponse
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
        public bool HasWarrantyRequest { get; set; } 
        public string PaymentMethod { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = string.Empty;
        public string? CancelReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
        public List<OrderShipmentResponse> Shipments { get; set; } = new();
        public bool HasUninstallBooking { get; set; } = false;
    }

    public class OrderItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string? VariantSku { get; set; }
        public string? VariantName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool RequiresInstallation { get; set; }
        public int? WarrantyPeriod { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class OrderShipmentResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Carrier { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WarehouseAllocationResponse
    {
        public int OrderItemId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    

    
}
