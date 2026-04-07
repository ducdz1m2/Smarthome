namespace Application.DTOs.Responses
{
    public class TechnicianResponse
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        
        // Thông tin cá nhân
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? IdentityCard { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        
        // Thông tin công việc
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal BaseSalary { get; set; }
        
        // Phân công & năng lực
        public string City { get; set; } = string.Empty;
        public List<string> Districts { get; set; } = new();
        public List<string> Skills { get; set; } = new();
        public bool IsAvailable { get; set; }
        
        // Thống kê
        public double Rating { get; set; }
        public int CompletedJobs { get; set; }
        public int CancelledJobs { get; set; }
    }
}
