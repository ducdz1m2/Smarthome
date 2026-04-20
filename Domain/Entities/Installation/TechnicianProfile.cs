namespace Domain.Entities.Installation;

using Domain.Abstractions;
using Domain.Exceptions;
using Domain.ValueObjects;
using Domain.Entities.Identity;

/// <summary>
/// TechnicianProfile aggregate root - represents a technician's profile and capabilities.
/// </summary>
public class TechnicianProfile : AggregateRoot
    {
        public int? UserId { get; private set; }

        // Personal info
        public string FullName { get; private set; } = string.Empty;
        public PhoneNumber PhoneNumber { get; private set; } = null!;
        public Email? Email { get; private set; }
        public string? IdentityCard { get; private set; } // CCCD
        public Address? Address { get; private set; }
        public DateTime? DateOfBirth { get; private set; }

        public virtual ApplicationUser? User { get; private set; }

        // Work info
        public string EmployeeCode { get; private set; } = string.Empty;
        public DateTime HireDate { get; private set; }
        public Money BaseSalary { get; private set; } = null!;
        
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
            PhoneNumber phoneNumber,
            string employeeCode,
            string city,
            List<string> districts,
            Email? email = null,
            string? identityCard = null,
            Address? address = null,
            DateTime? dateOfBirth = null,
            Money? baseSalary = null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ValidationException(nameof(fullName), "Họ tên không được trống");

            if (string.IsNullOrWhiteSpace(employeeCode))
                throw new ValidationException(nameof(employeeCode), "Mã nhân viên không được trống");

            if (string.IsNullOrWhiteSpace(city))
                throw new ValidationException(nameof(city), "Thành phố không được trống");

            if (!districts.Any())
                throw new ValidationException(nameof(districts), "Phải có ít nhất một khu vực phục vụ");

            return new TechnicianProfile
            {
                FullName = fullName.Trim(),
                PhoneNumber = phoneNumber,
                Email = email,
                IdentityCard = identityCard?.Trim(),
                Address = address,
                DateOfBirth = dateOfBirth,
                EmployeeCode = employeeCode.Trim().ToUpper(),
                HireDate = DateTime.UtcNow,
                BaseSalary = baseSalary ?? Money.Zero(),
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
            PhoneNumber phoneNumber,
            Email? email = null,
            string? identityCard = null,
            Address? address = null,
            DateTime? dateOfBirth = null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ValidationException(nameof(fullName), "Họ tên không được trống");

            FullName = fullName.Trim();
            PhoneNumber = phoneNumber;
            Email = email;
            IdentityCard = identityCard?.Trim();
            Address = address;
            DateOfBirth = dateOfBirth;
        }

        public void UpdateWorkInfo(Money baseSalary, string city, List<string> districts)
        {
            BaseSalary = baseSalary ?? Money.Zero();
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
