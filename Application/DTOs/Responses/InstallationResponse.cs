namespace Application.DTOs.Responses
{
    public class InstallationBookingResponse
    {
        public int Id { get; set; }
        
        // Thông tin đơn hàng
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
        
        // Thông tin khách hàng
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? District { get; set; }
        
        // Sản phẩm cần lắp
        public List<InstallationProductItem> Products { get; set; } = new();
        
        // Thông tin kỹ thuật viên
        public int? TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public string TechnicianPhone { get; set; } = string.Empty;
        
        // Thông tin lịch hẹn
        public int SlotId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        
        // Trạng thái
        public string Status { get; set; } = string.Empty;
        public bool MaterialsPrepared { get; set; }
        public DateTime? OnTheWayAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CustomerRating { get; set; }
        public string? CustomerSignature { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Vật tư
        public List<InstallationMaterialResponse> Materials { get; set; } = new();
    }

    public class InstallationProductItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class InstallationMaterialResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantityTaken { get; set; }
        public int? QuantityUsed { get; set; }
        public int? QuantityReturned { get; set; }
    }
}
