namespace Domain.Entities.Identity
{
    using Domain.Entities.Common;
    using Domain.Exceptions;

    public class AppRole : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }

        public virtual ICollection<AppUser> Users { get; private set; } = new List<AppUser>();

        private AppRole() { }

        public static AppRole Create(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên vai trò không được trống");

            if (name.Length > 50)
                throw new DomainException("Tên vai trò tối đa 50 ký tự");

            return new AppRole
            {
                Name = name.Trim(),
                Description = description?.Trim()
            };
        }

        public void Update(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên vai trò không được trống");

            Name = name.Trim();
            Description = description?.Trim();
        }
    }
}
