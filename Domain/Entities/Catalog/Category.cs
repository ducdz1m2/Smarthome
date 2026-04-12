using Domain.Abstractions;
using Domain.Exceptions;

namespace Domain.Entities.Catalog;

/// <summary>
/// Category entity - represents a product category in the catalog.
/// </summary>
public class Category : Entity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public int? ParentId { get; private set; }
        public int SortOrder { get; private set; }
        public bool IsActive { get; private set; } = true;

        public virtual Category? Parent { get; private set; }
        public virtual ICollection<Category> Children { get; private set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

        private Category() { }
        public static Category Create(string name, int? parentId = null, int sortOrder = 0, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException(nameof(name), "Tên danh mục không được trống");

            return new Category
            {
                Name = name.Trim(),
                Description = description?.Trim(),
                ParentId = parentId,
                SortOrder = sortOrder,
                IsActive = true
            };
        }

        public void Update(string name, int? parentId, int sortOrder, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException(nameof(name), "Tên danh mục không được trống");

            // Validate không tự làm con của chính mình
            if (parentId == Id)
                throw new BusinessRuleViolationException("CategorySelfReference", "Không thể đặt danh mục làm con của chính nó");

            Name = name.Trim();
            Description = description?.Trim();
            ParentId = parentId;
            SortOrder = sortOrder;
        }

        public void MoveTo(int? newParentId)
        {
            if (newParentId == Id)
                throw new BusinessRuleViolationException("CategorySelfReference", "Không thể di chuyển vào chính nó");

            ParentId = newParentId;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public bool HasChildren() => Children.Any();

        public bool HasProducts() => Products.Any();

        public void AddChild(string name)
        {
            var child = Create(name, Id);
            Children.Add(child);
        }

        public void UpdateSortOrder(int newOrder)
        {
            SortOrder = newOrder;
        }
    }
