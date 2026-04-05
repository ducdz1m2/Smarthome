namespace Application.DTOs
{
    public class InstallationBookingDto
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
        public List<InstallationMaterialDto> Materials { get; set; } = new();
    }

    public class InstallationMaterialDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantityTaken { get; set; }
        public int? QuantityUsed { get; set; }
        public int? QuantityReturned { get; set; }
    }

    public class BookInstallationRequest
    {
        public int OrderId { get; set; }
        public DateTime PreferredDate { get; set; }
        public TimeSpan? PreferredTime { get; set; }
        public string CustomerAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class TechnicianDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public List<string> Districts { get; set; } = new();
        public List<string> Skills { get; set; } = new();
        public bool IsAvailable { get; set; }
        public double? Rating { get; set; }
    }

    public class InstallationSlotDto
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsBooked { get; set; }
    }
}
