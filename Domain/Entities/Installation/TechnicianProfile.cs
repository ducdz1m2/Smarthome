namespace Domain.Entities.Installation
{
    using Domain.Entities.Common;
    using Domain.Exceptions;

    public class TechnicianProfile : BaseEntity
    {
        public int? UserId { get; private set; }
        
        // Thông tin cá nhân
        public string FullName { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public string? Email { get; private set; }
        public string? IdentityCard { get; private set; } // CCCD
        public string? Address { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        
        // Thông tin công việc
        public string EmployeeCode { get; private set; } = string.Empty; // Mã nhân viên
        public DateTime HireDate { get; private set; }
        public decimal BaseSalary { get; private set; }
        
        // Phân công & năng lực
        public string City { get; private set; } = string.Empty; // Hà Nội, TP.HCM...
        public string Districts { get; private set; } = string.Empty; // JSON: ["Q1","Q2","Q3"]
        public string SkillsJson { get; private set; } = "[]"; // ["Lắp khóa","Lắp camera"]
        public bool IsAvailable { get; private set; } = true;
        
        // Thống kê
        public double Rating { get; private set; } = 5.0;
        public int CompletedJobs { get; private set; } = 0;
        public int CancelledJobs { get; private set; } = 0;

        public virtual ICollection<InstallationSlot> Slots { get; private set; } = new List<InstallationSlot>();
        public virtual ICollection<InstallationBooking> Bookings { get; private set; } = new List<InstallationBooking>();

        private TechnicianProfile() { }

        public static TechnicianProfile Create(
            string fullName, 
            string phoneNumber, 
            string employeeCode,
            string city,
            List<string> districts,
            string? email = null,
            string? identityCard = null,
            string? address = null,
            DateTime? dateOfBirth = null,
            decimal baseSalary = 0)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("Họ tên không được trống");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new DomainException("Số điện thoại không được trống");

            if (string.IsNullOrWhiteSpace(employeeCode))
                throw new DomainException("Mã nhân viên không được trống");

            if (string.IsNullOrWhiteSpace(city))
                throw new DomainException("Thành phố không được trống");

            if (!districts.Any())
                throw new DomainException("Phải có ít nhất một khu vực phục vụ");

            return new TechnicianProfile
            {
                FullName = fullName.Trim(),
                PhoneNumber = phoneNumber.Trim(),
                Email = email?.Trim().ToLower(),
                IdentityCard = identityCard?.Trim(),
                Address = address?.Trim(),
                DateOfBirth = dateOfBirth,
                EmployeeCode = employeeCode.Trim().ToUpper(),
                HireDate = DateTime.UtcNow,
                BaseSalary = baseSalary,
                City = city.Trim(),
                Districts = System.Text.Json.JsonSerializer.Serialize(districts),
                SkillsJson = "[]",
                IsAvailable = true,
                Rating = 5.0,
                CompletedJobs = 0,
                CancelledJobs = 0
            };
        }

        public void UpdateInfo(
            string fullName, 
            string phoneNumber,
            string? email = null,
            string? identityCard = null,
            string? address = null,
            DateTime? dateOfBirth = null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("Họ tên không được trống");

            FullName = fullName.Trim();
            PhoneNumber = phoneNumber.Trim();
            Email = email?.Trim().ToLower();
            IdentityCard = identityCard?.Trim();
            Address = address?.Trim();
            DateOfBirth = dateOfBirth;
        }

        public void UpdateWorkInfo(decimal baseSalary, string city, List<string> districts)
        {
            BaseSalary = baseSalary;
            if (!string.IsNullOrWhiteSpace(city))
                City = city.Trim();
            if (districts.Any())
                Districts = System.Text.Json.JsonSerializer.Serialize(districts);
        }

        public void LinkToUser(int userId)
        {
            UserId = userId;
        }

        public void AddSkill(string skill)
        {
            var skills = GetSkills();
            if (!skills.Contains(skill))
            {
                skills.Add(skill);
                SkillsJson = System.Text.Json.JsonSerializer.Serialize(skills);
            }
        }

        public List<string> GetSkills()
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(SkillsJson) ?? new List<string>();
        }

        public List<string> GetDistricts()
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Districts) ?? new List<string>();
        }

        public bool CanHandle(string district, string? requiredSkill = null)
        {
            if (!IsAvailable) return false;

            var districts = GetDistricts();
            if (!districts.Contains(district)) return false;

            if (requiredSkill != null)
            {
                var skills = GetSkills();
                if (!skills.Contains(requiredSkill)) return false;
            }

            return true;
        }

        public void CompleteJob(int customerRating)
        {
            CompletedJobs++;
            UpdateRating(customerRating);
        }

        public void CancelJob()
        {
            CancelledJobs++;
            Rating = Math.Max(1.0, Rating - 0.1);
        }

        private void UpdateRating(int newRating)
        {
            var totalRating = Rating * (CompletedJobs - 1) + newRating;
            Rating = totalRating / CompletedJobs;
        }

        public void SetAvailable(bool available)
        {
            IsAvailable = available;
        }
    }
}
