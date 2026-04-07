namespace Application.DTOs.Requests
{
    public class CreateTechnicianProfileRequest
    {
        // Thông tin đăng nhập
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        // Thông tin cá nhân
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? IdentityCard { get; set; } // CCCD
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        
        // Thông tin công việc
        public string EmployeeCode { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        
        // Phân công - City -> Districts
        public string City { get; set; } = string.Empty; // Hà Nội, TP.HCM, Đà Nẵng...
        public List<string> Districts { get; set; } = new();
        public List<string> Skills { get; set; } = new();
    }

    public class UpdateTechnicianProfileRequest
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? IdentityCard { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public decimal? BaseSalary { get; set; }
        public string? City { get; set; }
        public List<string>? Districts { get; set; }
        public List<string>? Skills { get; set; }
        public bool? IsAvailable { get; set; }
    }

    public class AddTechnicianSkillRequest
    {
        public string Skill { get; set; } = string.Empty;
    }
}
