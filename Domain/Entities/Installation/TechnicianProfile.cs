namespace Domain.Entities.Installation
{
    using Domain.Entities.Common;
    using Domain.Exceptions;
    using Domain.ValueObjects;

    public class TechnicianProfile : BaseEntity
    {
        public int UserId { get; private set; }
        public string Districts { get; private set; } = string.Empty; // JSON: ["Q1","Q2","Q3"]
        public double Rating { get; private set; } = 5.0;
        public int CompletedJobs { get; private set; } = 0;
        public int CancelledJobs { get; private set; } = 0;
        public bool IsAvailable { get; private set; } = true;
        public string SkillsJson { get; private set; } = "[]"; // ["Lắp khóa","Lắp camera"]

        public virtual ICollection<InstallationSlot> Slots { get; private set; } = new List<InstallationSlot>();
        public virtual ICollection<InstallationBooking> Bookings { get; private set; } = new List<InstallationBooking>();

        private TechnicianProfile() { }

        public static TechnicianProfile Create(int userId, List<string> districts)
        {
            if (userId <= 0)
                throw new DomainException("UserId không hợp lệ");

            if (!districts.Any())
                throw new DomainException("Phải có ít nhất một khu vực phục vụ");

            return new TechnicianProfile
            {
                UserId = userId,
                Districts = System.Text.Json.JsonSerializer.Serialize(districts),
                Rating = 5.0,
                CompletedJobs = 0,
                CancelledJobs = 0,
                IsAvailable = true,
                SkillsJson = "[]"
            };
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
