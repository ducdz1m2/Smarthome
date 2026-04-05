namespace Application.DTOs.Responses
{
    public class InstallationBookingResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public int SlotId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool MaterialsPrepared { get; set; }
        public DateTime? OnTheWayAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CustomerRating { get; set; }
        public string? CustomerSignature { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<InstallationMaterialResponse> Materials { get; set; } = new();
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
