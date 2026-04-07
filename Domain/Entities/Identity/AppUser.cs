namespace Domain.Entities.Identity
{
    using Domain.Entities.Common;
    using Domain.Exceptions;

    public class AppUser : BaseEntity
    {
        public string UserName { get; private set; } = string.Empty;
        public string Email { get; private set; } = null!;
        public string? PhoneNumber { get; private set; }
        public string FullName { get; private set; } = string.Empty;
        public string? Avatar { get; private set; }
        public string PasswordHash { get; private set; } = string.Empty;
        public string PasswordSalt { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;
        public DateTime? LastLoginAt { get; private set; }

        public virtual ICollection<AppRole> Roles { get; private set; } = new List<AppRole>();
        public virtual ICollection<Entities.Content.UserAddress> Addresses { get; private set; } = new List<Entities.Content.UserAddress>();

        private AppUser() { }

        public static AppUser Create(string userName, string email, string fullName, string passwordHash, string passwordSalt, string? phone = null)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new DomainException("Tên đăng nhập không được trống");

            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("Họ tên không được trống");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new DomainException("Mật khẩu không được trống");

            return new AppUser
            {
                UserName = userName.Trim().ToLower(),
                Email = email.Trim().ToLower(),
                FullName = fullName.Trim(),
                PhoneNumber = phone?.Trim(),
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsActive = true
            };
        }

        public void UpdateProfile(string fullName, string? avatar)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("Họ tên không được trống");

            FullName = fullName.Trim();
            Avatar = avatar?.Trim();
        }

        public void UpdatePhone(string? phone)
        {
            PhoneNumber = phone?.Trim();
        }

        public void MarkLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void AssignRole(AppRole role)
        {
            if (!Roles.Any(r => r.Id == role.Id))
            {
                Roles.Add(role);
            }
        }

        public void RemoveRole(AppRole role)
        {
            var existing = Roles.FirstOrDefault(r => r.Id == role.Id);
            if (existing != null)
            {
                Roles.Remove(existing);
            }
        }

        public bool HasRole(string roleName)
        {
            return Roles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
