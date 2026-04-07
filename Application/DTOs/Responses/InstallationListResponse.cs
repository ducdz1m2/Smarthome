namespace Application.DTOs.Responses
{
    public class InstallationBookingListResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool MaterialsPrepared { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CustomerRating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TechnicianListResponse
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string City { get; set; } = string.Empty; 
        public List<string> Districts { get; set; } = new();
        public double Rating { get; set; }
        public int CompletedJobs { get; set; }
        public int CancelledJobs { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class InstallationSlotListResponse
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
