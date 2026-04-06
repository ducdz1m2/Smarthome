using Domain.Entities.Common;
using Domain.Exceptions;

namespace Domain.Entities.Catalog
{
    public class Brand : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string? LogoUrl { get; private set; }
        public string? Website { get; private set; }
        public bool IsActive { get; private set; } = true;

        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

        public static Brand Create(string name, string? description = null,
            string? logoUrl = null, string? website = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên thương hiệu không được trống");

            return new Brand
            {
                Name = name.Trim(),
                Description = description?.Trim(),
                LogoUrl = logoUrl?.Trim(),
                Website = website?.Trim(),
                IsActive = true
            };
        }

        public void Update(string name, string? description, string? logoUrl, string? website)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên thương hiệu không được trống");

            Name = name.Trim();
            Description = description?.Trim();
            LogoUrl = logoUrl?.Trim();
            Website = website?.Trim();
        }

       
    }
}
